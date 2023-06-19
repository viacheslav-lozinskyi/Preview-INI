using IniParser.Model;
using IniParser.Parser;
using System;
using System.IO;

namespace resource.preview
{
    internal class VSPreview : extension.AnyPreview
    {
        protected override void _Execute(atom.Trace context, int level, string url, string file)
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
                if (__Execute(context, level, a_Context, File.ReadAllText(file), ";")) return;
                if (__Execute(context, level, a_Context, File.ReadAllText(file), "#")) return;
            }
            if (a_Context.HasError && (GetState() != NAME.STATE.WORK.CANCEL))
            {
                foreach (var a_Context1 in a_Context.Errors)
                {
                    context.
                        Send(NAME.SOURCE.PREVIEW, NAME.EVENT.ERROR, level, a_Context1.Message);
                }
                {
                    context.
                        SendPreview(NAME.EVENT.ERROR, url);
                }
            }
        }

        private static bool __Execute(atom.Trace context, int level, IniDataParser parser, string data, string comment)
        {
            if (GetState() == NAME.STATE.WORK.CANCEL)
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
                    __Execute(context, level, a_Context);
                }
            }
            return parser.HasError == false;
        }

        private static void __Execute(atom.Trace context, int level, IniData data)
        {
            foreach (var a_Context in data.Sections)
            {
                if (GetState() == NAME.STATE.WORK.CANCEL)
                {
                    return;
                }
                else
                {
                    context.
                        SetComment("[[[Section]]]").
                        Send(NAME.SOURCE.PREVIEW, NAME.EVENT.PARAMETER, level, a_Context.SectionName);
                }
                foreach (var a_Context1 in a_Context.Keys)
                {
                    context.
                        SetComment(__GetComment(a_Context1.Value), "[[[Data Type]]]").
                        Send(NAME.SOURCE.PREVIEW, NAME.EVENT.PARAMETER, level + 1, a_Context1.KeyName, a_Context1.Value);
                }
            }
        }

        private static string __GetComment(string data)
        {
            {
                var a_Context = data.ToUpper();
                if ((a_Context == "TRUE") || (a_Context == "FALSE"))
                {
                    return "[[[Boolean]]]";
                }
            }
            {
                var a_Context = (Int64)0;
                if (Int64.TryParse(data, out a_Context))
                {
                    return "[[[Integer]]]";
                }
            }
            {
                var a_Context = (double)0;
                if (double.TryParse(data, out a_Context))
                {
                    return "[[[Double]]]";
                }
            }
            return "[[[String]]]";
        }
    };
}
