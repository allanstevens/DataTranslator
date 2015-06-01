using System;
using System.Collections.Generic;
using System.Text;

namespace AllanStevens.DataTranslator.Factory
{
    interface ITranslator
    {
        string TranslatorFilename { set; }
        string SourceFilename { set; }
        string TargetFilename { set; }

        void Initialize();

        void RunTranslator();

    }
}
