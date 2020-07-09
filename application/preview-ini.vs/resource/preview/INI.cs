
using IniParser.Model;
using IniParser.Parser;
using System;
using System.IO;

namespace resource.preview
{
    public class INI : cartridge.AnyPreview
    {
        protected override void _Execute(atom.Trace context, string url)
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
                if (__Execute(a_Context, context, File.ReadAllText(url), ";")) return;
                if (__Execute(a_Context, context, File.ReadAllText(url), "#")) return;
            }
            if (a_Context.HasError)
            {
                foreach (var a_Context1 in a_Context.Errors)
                {
                    context.
                        Clear().
                        SetContent(a_Context1.Message).
                        SetFlag(NAME.FLAG.ERROR).
                        SetLevel(1).
                        Send();
                }
            }
        }

        private static bool __Execute(IniDataParser parser, atom.Trace context, string data, string comment)
        {
            {
                parser.Configuration.CommentString = comment;
            }
            {
                var a_Context = parser.Parse(data);
                if (parser.HasError == false)
                {
                    __Execute(a_Context, context);
                }
            }
            return parser.HasError == false;
        }

        private static void __Execute(IniData data, atom.Trace context)
        {
            foreach (var a_Context in data.Sections)
            {
                {
                    context.
                        Clear().
                        SetContent(a_Context.SectionName).
                        SetComment("[[Section]]").
                        SetLevel(1).
                        Send();
                }
                foreach (var a_Context1 in a_Context.Keys)
                {
                    context.
                        Clear().
                        SetContent(a_Context1.KeyName).
                        SetValue(a_Context1.Value).
                        SetPattern(NAME.PATTERN.VARIABLE).
                        SetComment(__GetComment(a_Context1.Value)).
                        SetHint("[[Data type]]").
                        SetLevel(2).
                        Send();
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
