using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Runtime.Serialization;
using System.Reflection;

namespace LemmaSharp {
    [Serializable()]
    public class LemmatizerPrebuiltData : LemmatizerPrebuilt {

        #region Private Variables

        protected static string sFileMask = "wfl-{0}.tbl";

        private static Encoding[] aeLangEncoding = new Encoding[] {
            Encoding.UTF8,
            Encoding.UTF8,
            Encoding.GetEncoding("iso-8859-1"),
            Encoding.UTF8,
            Encoding.GetEncoding("iso-8859-1"),
            Encoding.GetEncoding("iso-8859-1"),
            Encoding.UTF8,
            Encoding.GetEncoding("iso-8859-1"),
            Encoding.UTF8,
            Encoding.UTF8,
            Encoding.UTF8,
            Encoding.GetEncoding("iso-8859-1")
        };

        #endregion

        #region Constructor(s) & Destructor(s)

        public LemmatizerPrebuiltData(LanguagePrebuilt lang):this(lang, new LemmatizerSettings()) {
        }

        public LemmatizerPrebuiltData(LanguagePrebuilt lang, LemmatizerSettings lsett)
            : base(lang,lsett) {

            Stream stream = GetResourceStream(ResourceFileName);
            StreamReader srIn = new StreamReader(stream, LanguageEncoding);
            AddMultextFile(srIn, "WLM");
            srIn.Close();

            BuildModel();
        }

        #endregion

        #region Private Properties

        protected string ResourceFileName {
            get {
                return GetResourceFileName(sFileMask);
            }
        }

        protected Encoding LanguageEncoding {
            get {
                return aeLangEncoding[(int)Language];
            }
        }
        #endregion

        #region Create Real Prebuild Lemmatizers for all languages

        public static void BuildAllLemmatizers(bool bIncludeExamples, bool bCompress, LemmatizerSettings lsett, string sNewFileMask) {
            DateTime dtStartAll = DateTime.Now;
            foreach (LanguagePrebuilt lp in Enum.GetValues(typeof(LanguagePrebuilt))) {
                DateTime dtStart = DateTime.Now;

                string sDataFileName = GetResourceFileName(sFileMask, lp);
                string sLemFileName = GetResourceFileName(sNewFileMask, lp);

                Console.WriteLine("Building lemmatizer for {0} from {1}", lp, sDataFileName);

                LemmatizerPrebuiltData lemPrebuild = new LemmatizerPrebuiltData(lp, lsett);

                Console.WriteLine("  Building lemmatizer completed in: {0}", new TimeSpan(DateTime.Now.Ticks - dtStart.Ticks).ToString());
                dtStart = DateTime.Now;
                Console.WriteLine("  Serializing {0} lemmatizer to file {1}", (bIncludeExamples ? "full (examples included)" : "compact (examples excluded)"), sLemFileName);

                Stream binStreamOut = File.Open(sLemFileName, FileMode.Create);
                lemPrebuild.Serialize(binStreamOut, bIncludeExamples, bCompress);
                binStreamOut.Close();

                Console.WriteLine("  Successfully completed, time needed: {0}", new TimeSpan(DateTime.Now.Ticks - dtStart.Ticks).ToString());
            }
            Console.WriteLine("Time for building all lemmatizers: " + new TimeSpan(DateTime.Now.Ticks - dtStartAll.Ticks).ToString());
        }

        #endregion

        #region Resource Management Functions

        protected override Assembly GetExecutingAssembly() {
            return Assembly.GetExecutingAssembly();
        }

        #endregion

    }

}
