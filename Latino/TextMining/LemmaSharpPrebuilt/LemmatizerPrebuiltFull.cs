using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Runtime.Serialization;
using System.Reflection;

namespace LemmaSharp {
    [Serializable()]
    public class LemmatizerPrebuiltFull : LemmatizerPrebuilt {

        #region Private Variables

        protected static string sFileMask = "full-{0}.lem";

        #endregion

        #region Constructor(s) & Destructor(s)

        public LemmatizerPrebuiltFull(LanguagePrebuilt lang): base(lang) {
            Stream stream = GetResourceStream(ResourceFileName);
            this.Deserialize(stream);
            stream.Close();
        }

        #endregion

        #region Private Properties

        protected string ResourceFileName {
            get {
                return GetResourceFileName(sFileMask);
            }
        }

        #endregion

        #region Resource Management Functions

        protected override Assembly GetExecutingAssembly() {
            return Assembly.GetExecutingAssembly();
        }

        #endregion

    }

}
