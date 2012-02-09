/***************************************************************************
 *    LemmaGen / LemmaSharp Library: Fast and efficient lemmatization.
 *    Copyright (C) 2009  Matjaz Jursic @ Jozef Stefan Institute
 *
 *    This program is free software: you can redistribute it and/or modify
 *    it under the terms of the GNU General Public License as published by
 *    the Free Software Foundation, either version 3 of the License, or
 *    (at your option) any later version.
 *
 *    This program is distributed in the hope that it will be useful,
 *    but WITHOUT ANY WARRANTY; without even the implied warranty of
 *    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *    GNU General Public License for more details.
 *
 *    You should have received a copy of the GNU General Public License
 *    along with this program.  If not, see <http://www.gnu.org/licenses/>.
 * 
 ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace LemmaSharp {
    [Serializable()]
    public class MsdSpec {
        #region SubClass Definitions
        
        public class Type {
            public string Name;
            public char Code;
            public int Attrs;
        }
        public class Attr {
            public string Name;
            public string Type;
            public int Pos;
        }

        public class Value {
            public string Name;
            public char Code;
            public string Attr;
            public string Type;
        } 

        #endregion

        #region Variables

        private string definition;

        Type[] Types;
        Attr[] Attrs;
        Value[] Values;

        Dictionary<string, int> typeNameToIdMap;
        Dictionary<char, int> typeCodeToIdMap;
        Dictionary<string, int> attrNameToIdMap;
        public Dictionary<int, string> attrIdToNameMap;
        Dictionary<int, Dictionary<int, Attr>> attrIdTypeIdAttr;
        Dictionary<int, Dictionary<int, Attr>> typeIdAttrIdAttr;
        Dictionary<int, Dictionary<int, Dictionary<char, Value>>> attrIdTypeIdValue;
        
        #endregion

        #region Properties

        public int AttrCount {
            get {
                return attrIdTypeIdAttr.Count;
            }
        }
        
        #endregion

        #region Constructor and Related Functions
		
        public MsdSpec(string definition) {
            ConstructFromDefinitionString(definition);
        }

        private void ConstructFromDefinitionString(string definition) {
            this.definition = definition;
            string[] definitionLines = definition.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None);
            LoadTablesFromDefinition(definitionLines);
            AddCategoryAttribute();
            ConstructTypeDicts();
            ConstructAttrDicts();
            ConstructValueDicts();
        }

        private void LoadTablesFromDefinition(string[] definitionLines) {
            List<Type> typesList = new List<Type>();
            List<Attr> attrsList = new List<Attr>();
            List<Value> valuesList = new List<Value>();

            int tableId = 0;
            bool inTable = false;
            //TODO this parsing is not really robust - shoul be improved
            foreach (string line in definitionLines) {
                if (line.StartsWith("*")) continue;

                if (string.IsNullOrEmpty(line)) {
                    inTable = false;
                }
                else {
                    if (!inTable) {
                        inTable = true;
                        tableId++;
                    }
                    string[] elements = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    switch (tableId) {
                        case 1:
                            typesList.Add(new Type() { Name = elements[0], Code = elements[1][0], Attrs = int.Parse(elements[2]) });
                            break;
                        case 2:
                            attrsList.Add(new Attr() { Name = elements[0], Type = elements[1], Pos = int.Parse(elements[2]) });
                            break;
                        case 3:
                            valuesList.Add(new Value() { Name = elements[0], Code = elements[1][0], Attr = elements[2], Type = elements[3] });
                            break;
                    }
                }
            }
            Types = typesList.ToArray();
            Attrs = attrsList.ToArray();
            Values = valuesList.ToArray();
        }
        private void AddCategoryAttribute() {
            List<Attr> newAttrs = new List<Attr>(Attrs);
            List<Value> newValues = new List<Value>(Values);

            //add base msd class also as an attrbute
            foreach (Type type in Types) {
                newAttrs.Add(new Attr() { Name = "Class", Type = type.Name, Pos = 0 });
                newValues.Add(new Value() { Name = type.Name, Attr = "Class", Type = type.Name, Code = type.Code });
            }
            Attrs = newAttrs.ToArray();
            Values = newValues.ToArray();
        }
        private void ConstructTypeDicts() {
            typeNameToIdMap = new Dictionary<string, int>();
            typeCodeToIdMap = new Dictionary<char, int>();
            for (int i = 0; i < Types.Length; i++) {
                Type type = Types[i];
                typeNameToIdMap[type.Name] = i;
                typeCodeToIdMap[type.Code] = i;
            }
        }
        private void ConstructAttrDicts() {
            attrNameToIdMap = new Dictionary<string, int>();
            attrIdToNameMap = new Dictionary<int, string>();
            attrIdTypeIdAttr = new Dictionary<int, Dictionary<int, Attr>>();
            typeIdAttrIdAttr = new Dictionary<int, Dictionary<int, Attr>>();
            for (int i = 0; i < Attrs.Length; i++) {
                Attr attr = Attrs[i];
                int attrId = 0;
                int typeId = 0;

                if (!attrNameToIdMap.TryGetValue(attr.Name, out attrId)) {
                    attrId = attrNameToIdMap.Count;
                    attrNameToIdMap.Add(attr.Name, attrId);
                    attrIdToNameMap.Add(attrId, attr.Name);
                }

                if (!typeNameToIdMap.TryGetValue(attr.Type, out typeId))
                    throw new Exception("The type specified in attributes table does not exist");

                if (!attrIdTypeIdAttr.ContainsKey(attrId))
                    attrIdTypeIdAttr[attrId] = new Dictionary<int, Attr>();
                if (!typeIdAttrIdAttr.ContainsKey(typeId))
                    typeIdAttrIdAttr[typeId] = new Dictionary<int, Attr>();

                if (attrIdTypeIdAttr[attrId].ContainsKey(typeId))
                    throw new Exception("Same attribute-type combination already added");

                attrIdTypeIdAttr[attrId][typeId] = attr;
                typeIdAttrIdAttr[typeId][attrId] = attr;
            }
        }
        private void ConstructValueDicts() {
            attrIdTypeIdValue = new Dictionary<int, Dictionary<int, Dictionary<char, Value>>>();

            for (int i = 0; i < Values.Length; i++) {
                Value val = Values[i];
                int attrId = 0;
                int typeId = 0;
                if (!attrNameToIdMap.TryGetValue(val.Attr, out attrId))
                    throw new Exception("The attribute specified in values table does not exist");
                if (!typeNameToIdMap.TryGetValue(val.Type, out typeId))
                    throw new Exception("The type specified in values table does not exist");
                Attr attr = attrIdTypeIdAttr[attrId][typeId];
                int attrPos = attr.Pos;

                if (!attrIdTypeIdValue.ContainsKey(attrId))
                    attrIdTypeIdValue[attrId] = new Dictionary<int, Dictionary<char, Value>>();
                if (!attrIdTypeIdValue[attrId].ContainsKey(typeId))
                    attrIdTypeIdValue[attrId][typeId] = new Dictionary<char, Value>();
                if (attrIdTypeIdValue[attrId][typeId].ContainsKey(val.Code))
                    throw new Exception("Same attribute-type-value combination already added");

                attrIdTypeIdValue[attrId][typeId][val.Code] = val;
            }
        }

	    #endregion        
        
        #region Supplementary Functions
		        
        public override string ToString() {
            StringBuilder sbTypes = new StringBuilder();

            int cumulCombs = 0;
            foreach (KeyValuePair<int, Dictionary<int, Attr>> typeDict in typeIdAttrIdAttr) {
                int combs = 1;
                Type type = Types[typeDict.Key];
                                
                StringBuilder sbAttrs = new StringBuilder();
                foreach (KeyValuePair<int, Attr> attDict in typeDict.Value) {
                    Attr att = attDict.Value;
                    int thisComb = attrIdTypeIdValue[attDict.Key][typeDict.Key].Count;
                    combs *= thisComb;

                    sbAttrs.Append(string.Format("       @{1}, {0}, Comb:{2} Values:", att.Name, att.Pos, thisComb));
                    
                    foreach (Value val in attrIdTypeIdValue[attDict.Key][typeDict.Key].Values) {
                        sbAttrs.Append(string.Format(" {0}[{1}]", val.Name, val.Code));
                    }
                    sbAttrs.AppendLine();
                }
                cumulCombs +=combs;

                sbTypes.AppendLine(string.Format("    {0}[{1}], {2} Attributes:, {3} Combinations", type.Name, type.Code, typeDict.Value.Count, combs));
                sbTypes.Append(sbAttrs);
            }

            StringBuilder sb = new StringBuilder();
            sb.AppendLine(string.Format("  Num of all possible msd combinations: {0}", cumulCombs));
            sb.AppendLine(string.Format("  Types:"));
            sb.Append(sbTypes);
            return sb.ToString();
        }

	    #endregion        
        
        #region Essential Clas Functionality Functions

        public char GetAttrValue(string msd, int attrId) {
            int typeId = 0;
            if (!typeCodeToIdMap.TryGetValue(msd[0], out typeId))
                throw new Exception("The type specified in msd is not predefined (" + msd + ")");
            if (!attrIdTypeIdAttr.ContainsKey(attrId))
                throw new Exception("The attribute id specified is not predefined (" + msd + ")");
            
            //TODO probably not correct usage of '*' and '-' CHECK!

            Attr attr = null;
            if (!attrIdTypeIdAttr[attrId].TryGetValue(typeId, out attr))
                return '*'; //return "not aplicable to this type"

            if (msd.Length <= attr.Pos) return '-';
            char valueCode = msd[attr.Pos];
            if (valueCode == '-') return '-';
            if (!attrIdTypeIdValue[attrId][typeId].ContainsKey(valueCode))
                throw new Exception("The attribute-type-valueCode combiantion specified in msd is not predefined (" + msd + ")");
            return valueCode;
        }

        #endregion

        #region Serialization Functions (Binary)

        public void Serialize(BinaryWriter binWrt) {
            binWrt.Write(definition);
        }
        public void Deserialize(BinaryReader binRead) {
            string definition = binRead.ReadString();
            ConstructFromDefinitionString(definition);
        }
        public MsdSpec(BinaryReader binRead) {
            Deserialize(binRead);
        }

        #endregion
    }
}
