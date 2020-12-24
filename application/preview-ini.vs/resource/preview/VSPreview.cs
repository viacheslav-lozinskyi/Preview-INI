
using IniParser.Model;
using IniParser.Parser;
using System;
using System.IO;

namespace resource.preview
{
    internal class VSPreview : cartridge.AnyPreview
    {
        protected override void _Execute(atom.Trace context, string url, int level)
        {
            var a_Context = new IniDataParser();
            {
                a_Context.Configuration.AllowDuplicateSections = true;
                a_Context.Configuration.ThrowExceptionsOnError = false;
                a_Context.Configuration.ConcatenateDuplicateKeys = true;
                a_Context.Configuration.OverrideDuplicateKeys = true;
                a_Context.Configuration.AllowDuplicateKeys = true;
                a_Context.Configuration.AllowKeysWithoutSection = true;
                a_Context.Configuration.CaseInsensitive = false;
                a_Context.Configuration.AllowCreateSectionsOnFly = true;
                a_Context.Configuration.SkipInvalidLines = true;
            }
            {
                if (__Execute(a_Context, context, level, File.ReadAllText(url), ";")) return;
                if (__Execute(a_Context, context, level, File.ReadAllText(url), "#")) return;
            }
            if (a_Context.HasError && (GetState() != STATE.CANCEL))
            {
                foreach (var a_Context1 in a_Context.Errors)
                {
                    context.
                        Send(NAME.SOURCE.PREVIEW, NAME.TYPE.ERROR, level, a_Context1.Message).
                        SendPreview(NAME.TYPE.ERROR, url);
                }
            }
        }

        private static bool __Execute(IniDataParser parser, atom.Trace context, int level, string data, string comment)
        {
            if (GetState() == STATE.CANCEL)
            {
                return false;
            }
            else
            {
                parser.Configuration.CommentString = comment;
            }
            {
                var a_Context = parser.Parse(data);
                if (parser.HasError == false)
                {
                    __Execute(a_Context, context, level);
                }
            }
            return parser.HasError == false;
        }

        private static void __Execute(IniData data, atom.Trace context, int level)
        {
            foreach (var a_Context in data.Sections)
            {
                if (GetState() == STATE.CANCEL)
                {
                    return;
                }
                else
                {
                    context.
                        SetComment("[[Section]]", "").
                        Send(NAME.SOURCE.PREVIEW, NAME.TYPE.INFO, level, a_Context.SectionName);
                }
                foreach (var a_Context1 in a_Context.Keys)
                {
                    context.
                        SetComment(__GetComment(a_Context1.Value), "[[Data type]]").
                        Send(NAME.SOURCE.PREVIEW, NAME.TYPE.VARIABLE, level + 1, a_Context1.KeyName, a_Context1.Value);
                }
            }
        }

        private static string __GetComment(string value)
        {
            {
                var a_Context = value.ToUpper();
                if ((a_Context == "TRUE") || (a_Context == "FALSE"))
                {
                    return "[[Boolean]]";
                }
            }
            {
                var a_Context = (Int64)0;
                if (Int64.TryParse(value, out a_Context))
                {
                    return "[[Integer]]";
                }
            }
            {
                var a_Context = (double)0;
                if (double.TryParse(value, out a_Context))
                {
                    return "[[Double]]";
                }
            }
            return "[[String]]";
        }
    };
}
