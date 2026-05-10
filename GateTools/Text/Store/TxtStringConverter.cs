using Gate.Tools.Extensions;
using Gate.Tools.Message;
using IronSoftware.Drawing;
using System.Globalization;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace Gate.Tools.Text
{
   /// <summary>
   /// Converte from/to <see cref="string"/> suitbale from <see cref="Gate.Tools.AppParams.AppParam"/> 
   /// </summary>
   public abstract class TxtStringConverter
   {
      public abstract string? ToStr(object? objValue);

      public abstract bool TryParse(string stringValue, Type stringType, out object? result, MsgCollection? msgs = null);

      public abstract class WithTypeCheck : TxtStringConverter
      {
         public abstract bool TypeCheck(Type type);
      }

      public abstract class Simple<T> : WithTypeCheck
      {
         public override bool TypeCheck(Type type) => type == typeof(T);

         public override string? ToStr(object? objValue) => objValue?.ToString();
      }

      public class ForEnum : WithTypeCheck
      {
         public override string? ToStr(object? objValue) => objValue?.ToString();

         public override bool TryParse(string stringValue, Type stringType, out object? result, MsgCollection? msgs)
         {
            try
            {
               result = Enum.Parse(stringType, stringValue);

               return true;
            }
            catch
            {
               msgs?.Add(new Msg(MsgType.fail, $"Enum {stringType} not containaing any label called '{stringValue}'"));
               result = null;

               return false;
            }
         }

         public override bool TypeCheck(Type type) => type.IsEnum;
      }

      public class ForCultureInfo : WithTypeCheck
      {
         public override string? ToStr(object? objValue) => (objValue as CultureInfo)?.Name;

         public override bool TryParse(string stringValue, Type stringType, out object? result, MsgCollection? msgs = null)
         {
            try
            {
               result = CultureInfo.GetCultureInfo(stringValue);

               return true;
            }
            catch (System.Globalization.CultureNotFoundException)
            {
               msgs?.Add(new Msg(MsgType.fail, $"", null, null));
               result = null;

               return false;
            }
            catch (Exception exc) { throw new Crash(exc); }
         }

         public override bool TypeCheck(Type type) => type == typeof(CultureInfo);
      }

      public class Default : TxtStringConverter
      {
         private readonly static WithTypeCheck[] myWithTypesDefault;

         static Default()
         {
            var ass = AppDomain.CurrentDomain.GetAssemblies();

            var cls = ass.SelectMany(
               a => a.GetTypes().
                  Where(t => t.IsSubclassOf(typeof(WithTypeCheck)) && t.IsBuildable()).Select(t => (WithTypeCheck)t.InstanciateOrCrash())).ToArray();

            myWithTypesDefault = cls;
         }

         private class FromParseMethod : TxtStringConverter
         {
            public static FromParseMethod Instance => new FromParseMethod();

            public override bool TryParse(string stringValue, Type stringType, out object? result, MsgCollection? msgs)
            {
               var mth = stringType.GetMethod("Parse", new Type[] { typeof(string) });

               if (mth == null) { throw new Crash($"Not a 'Parse(string)' method for type {stringType}"); }
               else
               {
                  try
                  {
                     result = mth.Invoke(null, [stringValue]);

                     return true;
                  }
                  catch (TargetInvocationException exc)
                  {
                     return myAddErrorMessage(out result, msgs, exc.InnerException ?? throw new Crash(), stringValue, stringType);
                  }
                  catch (Exception exc) { return myAddErrorMessage(out result, msgs, exc, stringValue, stringType); }
               }
            }

            private static bool myAddErrorMessage(
               out object? result, MsgCollection? msgs, Exception exception, string stringValue, Type stringType)
            {
               if (msgs == null)
               {
                  Console.WriteLine($"During parsing of {stringValue} to {stringType.Name}");
                  Console.WriteLine(exception?.Message);
               }
               else
               {
                  msgs?.Add(new Msg(MsgType.fail, $"During parsing of {stringValue} to {stringType.Name}", null, null));
                  msgs?.Add(new Msg(MsgType.fail, exception?.Message, null, null));
               }

               result = null;

               return false;
            }

            public override string? ToStr(object? objValue) => objValue?.ToString();
         }

         public class ForString : Simple<string>
         {
            public override bool TryParse(string stringValue, Type stringType, out object result, MsgCollection? msgs)
            {
               result = stringValue;

               return true;
            }
         }

         /// <summary>
         /// <see cref="TxtStringConverter"/> for <see cref="IronSoftware.Drawing.Font"/>
         /// </summary>
         public class ForIronSoftwareFont : Simple<IronSoftware.Drawing.Font>
         {
            public ForIronSoftwareFont() { }

            public override bool TryParse(string stringValue, Type stringType, out object? result, MsgCollection? msgs)
            {
               if (stringValue.TryParseFont(out var fnt))
               {
                  result = fnt;

                  return true;
               }
               else
               {
                  result = null;

                  return false;
               }
            }

            public override string ToStr(object? objValue) => objValue is Font fnt ? fnt.FontToString() : throw new Crash($"Not a Font!");
         }

         public class ForEncoding : WithTypeCheck
         {
            private static EncodingInfo[] myEncodings = Encoding.GetEncodings();

            public override string? ToStr(object? objValue) => (objValue as Encoding)?.HeaderName;

            public override bool TryParse(string stringValue, Type stringType, out object? result, MsgCollection? msgs)
            {
               var inf = myEncodings.FirstOrDefault(i => i.Name == stringValue);

               if (inf != null)
               {
                  result = inf.GetEncoding();

                  return true;
               }
               else
               {
                  msgs?.Add(new Msg(MsgType.fail, $"Can't translate '{stringValue}' to an Encoding"));

                  result = null;

                  return false;
               }
            }

            public override bool TypeCheck(Type type) => type == typeof(Encoding) || type.IsSubclassOf(typeof(Encoding));
         }

         public class ForColor : Simple<System.Drawing.Color>
         {
            public override bool TryParse(string stringValue, Type stringType, out object? result, MsgCollection? msgs)
            {
               var ps2 = @"Color\[A\=(?<a>\d+)\,R\=(?<r>\d+)\,G\=(?<g>\d+)\,B\=(?<b>\d+)\]";
               var ps1 = string.Format(@"Color\[(?<c>({0}))]", string.Join("|", Enum.GetNames(typeof(System.Drawing.KnownColor))));
               var rgx = new Regex(string.Format("({0})|({1})", ps2, ps1), RegexOptions.IgnoreCase | RegexOptions.Compiled);
               var sp_rgx = new Regex(@"\s*");
               var wrk_str = sp_rgx.Replace(stringValue, "");

               var mat = rgx.Match(wrk_str);

               if (mat.Success && mat.Length == wrk_str.Length)
               {
                  if (mat.Groups["c"].Value != "")
                  {
                     result = System.Drawing.Color.FromKnownColor((System.Drawing.KnownColor)Enum.Parse(typeof(System.Drawing.KnownColor), mat.Groups["c"].Value));

                     return true;
                  }
                  else
                  {
                     var col =
                        System.Drawing.Color.FromArgb(
                           int.Parse(mat.Groups["a"].Value),
                           int.Parse(mat.Groups["r"].Value),
                           int.Parse(mat.Groups["g"].Value),
                           int.Parse(mat.Groups["b"].Value));

                     result = col.IsKnownColor ? System.Drawing.Color.FromKnownColor(col.ToKnownColor()) : col;

                     return true;
                  }
               }

               msgs?.Add(new Msg(MsgType.fail, $"Can't translate '{stringValue}' to Color", null, null));
               result = null;

               return false;
            }
         }

         public virtual WithTypeCheck[] WithTypeChecks => myWithTypesDefault;

         public override string? ToStr(object? objValue)
         {
            if (objValue == null) { return null; }
            else if (WithTypeChecks.Any(c => c.TypeCheck(objValue.GetType())))
            {
               return WithTypeChecks.First(c => c.TypeCheck(objValue.GetType())).ToStr(objValue);
            }
            else { return FromParseMethod.Instance.ToStr(objValue); }
         }

         public override bool TryParse(string stringValue, Type fieldType, out object? result, MsgCollection? msgs)
         {
            var und_typ = Nullable.GetUnderlyingType(fieldType);

            //in this case field type is Nullable (eg int?)
            if (und_typ != null)
            {
               // If is blank returns null (Nullable without value)
               if (stringValue.IsBlank())
               {
                  result = null;

                  return true;
               }
               else
               {
                  //parses value of nullable
                  if (TryParse(stringValue, und_typ, out var ir, msgs))
                  {
                     // Build the nullable using the result
                     var nul_val = Activator.CreateInstance(fieldType, ir);

                     result = nul_val;

                     return true;
                  }
                  else
                  {
                     result = null;

                     return false;
                  }
               }
            }

            if (WithTypeChecks.Any(c => c.TypeCheck(fieldType)))
            {
               return WithTypeChecks.First(c => c.TypeCheck(fieldType)).TryParse(stringValue, fieldType, out result, msgs);
            }
            else
            {
               return FromParseMethod.Instance.TryParse(stringValue, fieldType, out result, msgs);
            }
         }
      }
   }
}
