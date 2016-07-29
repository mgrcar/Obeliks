/***************************************************************************
 *    LemmaGen / LemmaSharp Library: Fast and efficient lemmatization.
 *    
 *    The MIT License (MIT)
 *    Copyright (C) 2009  Matjaz Jursic @ Jozef Stefan Institute
 *
 *    Permission is hereby granted, free of charge, to any person obtaining 
 *    a copy of this software and associated documentation files (the 
 *    "Software"), to deal in the Software without restriction, including 
 *    without limitation the rights to use, copy, modify, merge, publish, 
 *    distribute, sublicense, and/or sell copies of the Software, and to 
 *    permit persons to whom the Software is furnished to do so, subject to 
 *    the following conditions:
 *
 *    The above copyright notice and this permission notice shall be 
 *    included in all copies or substantial portions of the Software.
 *
 *    THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, 
 *    EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF 
 *    MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. 
 *    IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY 
 *    CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, 
 *    TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE 
 *    SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 * 
 ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Runtime.Serialization;

namespace LemmaSharp {
    public class LemmaRule {
        #region Private Variables

        private int iId;
        private int iFrom;
        private string sFrom;
        private string sTo;
        private string sSignature;
        private LemmatizerSettings lsett;

        #endregion

        #region Constructor(s) & Destructor(s)

        public LemmaRule(string sWord, string sLemma, int iId, LemmatizerSettings lsett) {
            this.lsett = lsett;
            this.iId = iId;

            int iSameStem = SameStem(sWord, sLemma);
            sTo = sLemma.Substring(iSameStem);
            iFrom = sWord.Length - iSameStem;

            if (lsett.bUseFromInRules) {
                sFrom = sWord.Substring(iSameStem);
                sSignature = "[" + sFrom + "]==>[" + sTo + "]";
            }
            else {
                sFrom = null;
                sSignature = "[#" + iFrom + "]==>[" + sTo + "]";
            }
        }

        #endregion

        #region Public Properties

        public string Signature {
            get {
                return sSignature;
            }
        }
        public int Id {
            get {
                return iId;
            }
        }

        #endregion

        #region Essential Class Functions

        private static int SameStem(string sStr1, string sStr2) {
            int iLen1 = sStr1.Length;
            int iLen2 = sStr2.Length;
            int iMaxLen = Math.Min(iLen1, iLen2);

            for (int iPos = 0; iPos < iMaxLen; iPos++)
                if (sStr1[iPos] != sStr2[iPos]) return iPos;

            return iMaxLen;
        }
        public bool IsApplicableToGroup(int iGroupCondLen) {
            return iGroupCondLen >= iFrom; 
        }
        public string Lemmatize(string sWord) {
            return sWord.Substring(0, sWord.Length - iFrom) + sTo;
        }

        #endregion

        #region Output Functions (ToString)

        public override string ToString() {
            return iId + ":" + sSignature;
        }

        #endregion

        #region Serialization Functions (Binary)

        public void Serialize(BinaryWriter binWrt, bool bThisTopObject) {
            //save metadata
            binWrt.Write(bThisTopObject);            
            
            //save value types --------------------------------------
            binWrt.Write(iId);
            binWrt.Write(iFrom);
            if (sFrom == null) 
                binWrt.Write(false);
            else {
                binWrt.Write(true);
                binWrt.Write(sFrom);
            }
            binWrt.Write(sTo);
            binWrt.Write(sSignature);

            if (bThisTopObject)
                lsett.Serialize(binWrt);
        }
        public void Deserialize(BinaryReader binRead, LemmatizerSettings lsett) {
            //load metadata
            bool bThisTopObject = binRead.ReadBoolean();    
            
            //load value types --------------------------------------
            iId = binRead.ReadInt32();
            iFrom = binRead.ReadInt32();
            if (binRead.ReadBoolean())
                sFrom = binRead.ReadString();
            else
                sFrom = null;
            sTo = binRead.ReadString();
            sSignature = binRead.ReadString();

            //load refernce types if needed -------------------------
            if (bThisTopObject)
                this.lsett = new LemmatizerSettings(binRead);
            else
                this.lsett = lsett;
        }
        public LemmaRule(System.IO.BinaryReader binRead, LemmatizerSettings lsett) {
            this.Deserialize(binRead, lsett);
        }

        #endregion
        #region Serialization Functions (Latino)
        #if LATINO

        public void Save(Latino.BinarySerializer binWrt, bool bThisTopObject) {
            //save metadata
            binWrt.WriteBool(bThisTopObject);

            //save value types --------------------------------------
            binWrt.WriteInt(iId);
            binWrt.WriteInt(iFrom);
            if (sFrom == null)
                binWrt.WriteBool(false);
            else {
                binWrt.WriteBool(true);
                binWrt.WriteString(sFrom);
            }
            binWrt.WriteString(sTo);
            binWrt.WriteString(sSignature);

            if (bThisTopObject)
                lsett.Save(binWrt);
        }
        public void Load(Latino.BinarySerializer binRead, LemmatizerSettings lsett) {
            //load metadata
            bool bThisTopObject = binRead.ReadBool();

            //load value types --------------------------------------
            iId = binRead.ReadInt();
            iFrom = binRead.ReadInt();
            if (binRead.ReadBool())
                sFrom = binRead.ReadString();
            else
                sFrom = null;
            sTo = binRead.ReadString();
            sSignature = binRead.ReadString();

            //load refernce types if needed -------------------------
            if (bThisTopObject)
                this.lsett = new LemmatizerSettings(binRead);
            else
                this.lsett = lsett;
        }
        public LemmaRule(Latino.BinarySerializer binRead, LemmatizerSettings lsett) {
            Load(binRead, lsett);
        }

        #endif
        #endregion
    }
}
