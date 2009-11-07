using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Runtime.Serialization;
using System.IO.Compression;

namespace LemmaSharp {
    [Serializable()]
    public class Lemmatizer : ITrainableLemmatizer 
        #if LATINO
        , Latino.ISerializable 
        #endif 
        {

        #region Private Variables

        protected LemmatizerSettings lsett;
        protected ExampleList elExamples;
        protected LemmaTreeNode ltnRootNode;

        #endregion

        #region Constructor(s) & Destructor(s)

        public Lemmatizer() : this(new LemmatizerSettings()) { }
        public Lemmatizer(LemmatizerSettings lsett) {
            this.lsett = lsett;
            this.elExamples = new ExampleList(lsett);
            this.ltnRootNode = null;
        } 
        public Lemmatizer(StreamReader srIn, string sFormat, LemmatizerSettings lsett): this(lsett) {
            AddMultextFile(srIn, sFormat);
        }

        #endregion

        #region Private Properties

        private LemmaTreeNode ltrRootNodeSafe {
            get {
                if (ltnRootNode == null) BuildModel();
                return ltnRootNode;
            }
        }

        #endregion
        #region Public Properties

        public LemmatizerSettings Settings{
            get{
                return lsett;
            }
        }
        public ExampleList Examples {
            get {
                return elExamples;
            }
        }
        public RuleList Rules {
            get {
                return elExamples.Rules;
            }
        }
        public LemmaTreeNode RootNode {
            get {
                return ltrRootNodeSafe;
            }
        }
        public ILemmatizerModel Model {
            get {
                return ltrRootNodeSafe;
            }
        }

        #endregion

        #region Essential Class Functions (adding examples to repository)

        public void AddMultextFile(StreamReader srIn, string sFormat) {
            this.elExamples.AddMultextFile(srIn, sFormat);
            ltnRootNode = null;
        }
        public void AddExample(string sWord, string sLemma) {
            AddExample(sWord, sLemma, 1, null);
        }
        public void AddExample(string sWord, string sLemma, double dWeight) {
            AddExample(sWord, sLemma, dWeight, null);
        }
        public void AddExample(string sWord, string sLemma, double dWeight, string sMsd) {
            elExamples.AddExample(sWord, sLemma, dWeight, sMsd);
            ltnRootNode = null;
        }
        public void DropExamples() {
            elExamples.DropExamples();
        }
        public void FinalizeAdditions() {
            elExamples.FinalizeAdditions();
        }

        #endregion
        #region Essential Class Functions (building model & lemmatizing)

        public void BuildModel() {
            if (ltnRootNode != null) return;
            //TODO remove: elExamples.FinalizeAdditions();
            ltnRootNode = new LemmaTreeNode(lsett, elExamples);
        }
        public string Lemmatize(string sWord) {
            return ltrRootNodeSafe.Lemmatize(sWord);
        }

        #endregion

        #region Serialization Functions (ISerializable)

        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue("lsett", lsett);
            info.AddValue("elExamples", elExamples);
        }
        public Lemmatizer(SerializationInfo info, StreamingContext context): this() {
            lsett = (LemmatizerSettings)info.GetValue("lsett", typeof(LemmatizerSettings));
            elExamples = (ExampleList)info.GetValue("elExamples", typeof(ExampleList));
            this.BuildModel();
        }

        #endregion
        #region Serialization Functions (Binary)

        public void Serialize(BinaryWriter binWrt, bool bSerializeExamples) {
            lsett.Serialize(binWrt);
            elExamples.Serialize(binWrt, bSerializeExamples, false);
            ltnRootNode.Serialize(binWrt);
        }
        public void Deserialize(BinaryReader binRead) {
            lsett = new LemmatizerSettings(binRead);
            elExamples = new ExampleList(binRead, lsett);
            ltnRootNode = new LemmaTreeNode(binRead, lsett, elExamples, null);            
        }
        public Lemmatizer(BinaryReader binRead) {
            Deserialize(binRead);
        }
        public Lemmatizer(Stream streamIn) : this(streamIn, false) { }
        public Lemmatizer(Stream streamIn, bool bCompress) {
            Stream streamInNew;
            if (bCompress)
                streamInNew = new DeflateStream(streamIn, CompressionMode.Decompress);
            else
                streamInNew = streamIn;

            BinaryReader binRead = new BinaryReader(streamInNew);
            Deserialize(binRead);
            binRead.Close();

            streamInNew.Close();
        }

        public void Serialize(Stream streamOut) {
            Serialize(streamOut, false, false);
        }
        public void Serialize(Stream streamOut, bool bSerializeExamples) {
            Serialize(streamOut, bSerializeExamples, false);
        }
        public void Serialize(Stream streamOut, bool bSerializeExamples, bool bCompress) {
            Stream streamOutNew;
            if (bCompress)
                streamOutNew = new DeflateStream(streamOut, CompressionMode.Compress);
            else
                streamOutNew = streamOut;

            BinaryWriter binWrt = new BinaryWriter(streamOutNew);
            this.Serialize(binWrt, bSerializeExamples);
            binWrt.Close();

            streamOutNew.Close();
        }
        public void Deserialize(Stream streamIn) {
            Deserialize(streamIn, false);
        }
        public void Deserialize(Stream streamIn, bool bCompress) {
            Stream streamInNew;
            if (bCompress)
                streamInNew = new DeflateStream(streamIn, CompressionMode.Decompress);
            else
                streamInNew = streamIn;

            BinaryReader binRead = new BinaryReader(streamInNew);
            Deserialize(binRead);
            binRead.Close();

            streamInNew.Close();
        }

        #endregion
        #region Serialization Functions (Latino)
        
        #if LATINO

        public void Save(Latino.BinarySerializer binWrt) {
            lsett.Save(binWrt);
            elExamples.Save(binWrt, true, false);
            ltnRootNode.Save(binWrt);
        }

        public void Load(Latino.BinarySerializer binRead) {
            lsett = new LemmatizerSettings(binRead);
            elExamples = new ExampleList(binRead, lsett);
            ltnRootNode = new LemmaTreeNode(binRead, lsett, elExamples, null);                
        }

        public Lemmatizer(Latino.BinarySerializer binRead) {
            Load(binRead);
        }

        public void Save(Stream streamOut) {
            Latino.BinarySerializer binWrt = new Latino.BinarySerializer(streamOut);
            this.Save(binWrt);
            binWrt.Close();
        }
        public void Load(Stream streamIn) {
            Latino.BinarySerializer binRead = new Latino.BinarySerializer(streamIn);
            Load(binRead);
            binRead.Close();
        }

        public Lemmatizer(Stream streamIn, string sDummy) {
            Load(streamIn);
        }

        #endif

        #endregion
    }
}
