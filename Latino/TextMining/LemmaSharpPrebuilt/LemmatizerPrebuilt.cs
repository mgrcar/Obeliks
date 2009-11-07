using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Runtime.Serialization;
using System.Reflection;

namespace LemmaSharp {

    public enum LanguagePrebuilt {
        Bulgarian,
        Czech,
        English,
        Estonian,
        French,
        German,
        Hungarian,
        Italian,
        Romanian,
        Serbian,
        Slovene,
        Spanish
    }

    [Serializable()]
    public abstract class LemmatizerPrebuilt : Lemmatizer {

        #region Private Variables

        private static string[] asLangMapping = new string[] {
            "me-bg",
            "me-cs",
            "m-en",
            "me-et",
            "m-fr",
            "m-ge",
            "me-hu",
            "m-it",
            "me-ro",
            "me-sr",
            "me-sl",
            "m-sp"
        };

        private LanguagePrebuilt lang;

        #endregion

        #region Constructor(s) & Destructor(s)

        public LemmatizerPrebuilt(LanguagePrebuilt lang)
            : base() {
            this.lang = lang;
        }

        public LemmatizerPrebuilt(LanguagePrebuilt lang, LemmatizerSettings lsett)
            : base(lsett) {
            this.lang = lang;
        }

        #endregion

        #region Private Properties Helping Functions

        protected string GetResourceFileName(string sFileMask) {
            return String.Format(sFileMask, asLangMapping[(int)lang]);
        }

        protected static string GetResourceFileName(string sFileMask, LanguagePrebuilt lang) {
            return String.Format(sFileMask, asLangMapping[(int)lang]);
        }

        #endregion

        #region Public Properties

        public LanguagePrebuilt Language {
            get{
                return lang;
            }
        }

        #endregion

        #region Resource Management Functions

        protected abstract Assembly GetExecutingAssembly();

        protected Stream GetResourceStream(string sResourceShortName) {
            Assembly assembly = GetExecutingAssembly();

            string sResourceName = null;
            foreach (string sResource in assembly.GetManifestResourceNames())
                if (sResource.EndsWith(sResourceShortName)) {
                    sResourceName = sResource;
                    break;
                }

            if (String.IsNullOrEmpty(sResourceName)) return null;

            return assembly.GetManifestResourceStream(sResourceName);
        }

        #endregion

        #region Serialization Functions

        public LemmatizerPrebuilt(SerializationInfo info, StreamingContext context)
            : base(info, context) {
        }

        #endregion

    }

}
