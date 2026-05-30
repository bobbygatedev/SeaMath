using Gate.LangBase.ExtraTypes;
using Gate.LangBase.Runtime.DbgEngVirtCpu;
using Gate.LangBase.Runtime.Object;
using Gate.Tools;
using Gate.Tools.Extensions;
using GateLangBase;
using System.Globalization;

namespace Gate.LangBase.Runtime
{
   /// <summary>
   /// The RtmFormatBase class provides a base implementation of the RtmFormat class, 
   /// which is responsible for formatting runtime objects for display purposes. 
   /// It includes a nested FormatVisitor class that uses the Visitor design pattern to provide string representations of various types of runtime objects. 
   /// The BuiltInVisitor class within FormatVisitor is responsible for providing string representations of built-in types, such as booleans and numeric values. 
   /// The RtmFormatBase class can be extended to provide specific formatting logic for different types of runtime objects by overriding the appropriate methods in the FormatVisitor and BuiltInVisitor classes.
   /// </summary>
   public class RtmFormatBase : RtmFormat
   {
      private const int STRING_MAX_CHARS = 128;

      public RtmFormatBase() { }

      /// <summary>
      /// The FormatVisitor class is responsible for providing string representations of various types of runtime objects. 
      /// It uses the Visitor design pattern to allow for different formatting logic based on the type of the runtime object being visited. 
      /// The Visit method is the entry point for visiting a runtime object, and it delegates to specific Visit methods based on the type of the object. 
      /// The BuiltInVisitor property provides access to an instance of the BuiltInVisitor class, which is used to format built-in types.
      /// </summary>
      public class FormatVisitor
      {
         public FormatVisitor(BuiltInVisitor builtInVisitor) => BuiltinVisitor = builtInVisitor;

         /// <summary>
         /// Visits a runtime object and returns its string representation. This method uses dynamic dispatch to call the appropriate Visit method based on the type of the runtime object. The default implementation calls the VisitDisplayValue method of the BuiltInVisitor class, passing the C# object representation of the runtime object and a flag indicating that it is being formatted for a scalar context.
         /// </summary>
         /// <param name="runTimeObj">The runtime object to be visited.</param>
         /// <returns>The string representation of the runtime object.</returns>
         public virtual string Visit(RtmObj runTimeObj) => BuiltinVisitor.VisitDisplayValue((dynamic)(runTimeObj.CSharpObj.NnOrCrash()), true);

         /// <summary>
         /// Visits a runtime function object and returns its string representation. 
         /// The default implementation returns a string that includes the identifier of the function declaration associated with the runtime function object.
         /// </summary>
         /// <param name="objFunction"></param>
         /// <returns></returns>
         public virtual string Visit(RtmDbgEngVirtCpuFunction objFunction) => $"Function {objFunction.Decl?.Identifier}";

         /// <summary>
         /// Provides access to an instance of the BuiltInVisitor class, 
         /// which is used to format built-in types. 
         /// This property is initialized in the constructor of the FormatVisitor class and can be overridden in derived classes to provide a different implementation of the BuiltInVisitor if needed.
         /// </summary>
         public BuiltInVisitor BuiltinVisitor { get; }
      }

      /// <summary>
      /// The BuiltInVisitor class is responsible for providing string representations of built-in types. It includes methods for formatting the display value of various types, such as booleans, integers, and floating-point numbers. The class uses a NumberFormatInfo instance to ensure that numeric values are formatted with a consistent decimal separator ("."). The VisitDisplayValue method is intended to be overridden for specific types, and will throw a Crash exception if called with an unsupported type.
      /// </summary>
      public class BuiltInVisitor
      {
         private readonly static NumberFormatInfo myNumberFormatInfo = new NumberFormatInfo();

         static BuiltInVisitor() => myNumberFormatInfo.NumberDecimalSeparator = ".";

         public string GetTypeRepresentation(Type type)
         {
            if (type == typeof(bool)) { return "boo"; }
            else if (UniversalInt.TryGetRepresentation(type, out var typ_i)) { return typ_i ?? throw new Crash(); }
            else if (UniversalNumeric.TryGetRepresentation(type, out var typ_f)) { return typ_f ?? throw new Crash(); }
            else { throw new Crash(); }
         }

         /// <summary>
         /// Formats the display value of an object. This method is intended to be overridden for specific types, and will throw a Crash exception if called with an unsupported type.
         /// </summary>
         /// <param name="trash"></param>
         /// <param name="isForScalar"></param>
         /// <returns></returns>
         /// <exception cref="Crash"></exception>
         public virtual string VisitDisplayValue(object trash, bool isForScalar) => throw new Crash();

         /// <summary>
         /// Formats the display value of a boolean. The value is converted to a lowercase string ("true" or "false") for display purposes.
         /// </summary>
         /// <param name="value">Value to be displayed.</param>
         /// <param name="isForScalar">If true, the value is being formatted for a scalar context.</param>
         /// <returns>String value to be displayed.</returns>
         public virtual string VisitDisplayValue(bool value, bool isForScalar) => value.ToString().ToLower();

         /// <summary>
         /// Formats the display value of a boolean. The value is converted to a lowercase string ("true" or "false") for display purposes.
         /// </summary>
         /// <param name="value">Value to be displayed.</param>
         /// <param name="isForScalar">If true, the value is being formatted for a scalar context.</param>
         /// <returns></returns>
         public virtual string VisitDisplayValue(sbyte value, bool isForScalar) => FormatInt(value, isForScalar);

         /// <summary>
         /// Formats the display value of a boolean. The value is converted to a lowercase string ("true" or "false") for display purposes.
         /// </summary>
         /// <param name="value">Value to be displayed.</param>
         /// <param name="isForScalar">If true, the value is being formatted for a scalar context.</param>
         /// <returns>String value to be displayed.</returns>
         public virtual string VisitDisplayValue(short value, bool isForScalar) => FormatInt(value, isForScalar);

         /// <summary>
         /// Formats the display value of a boolean. The value is converted to a lowercase string ("true" or "false") for display purposes.
         /// </summary>
         /// <param name="value">Value to be displayed.</param>
         /// <param name="isForScalar">If true, the value is being formatted for a scalar context.</param>
         /// <returns>String value to be displayed.</returns>
         public virtual string VisitDisplayValue(int value, bool isForScalar) => FormatInt(value, isForScalar);

         /// <summary>
         /// Formats the display value of a boolean. The value is converted to a lowercase string ("true" or "false") for display purposes.
         /// </summary>
         /// <param name="value">Value to be displayed.</param>
         /// <param name="isForScalar">If true, the value is being formatted for a scalar context.</param>
         /// <returns>String value to be displayed.</returns>
         public virtual string VisitDisplayValue(long value, bool isForScalar) => FormatInt(value, isForScalar);

         /// <summary>
         /// Formats the display value of a boolean. The value is converted to a lowercase string ("true" or "false") for display purposes.
         /// </summary>
         /// <param name="value">Value to be displayed.</param>
         /// <param name="isForScalar">If true, the value is being formatted for a scalar context.</param>
         /// <returns>String value to be displayed.</returns>
         public virtual string VisitDisplayValue(byte value, bool isForScalar) => FormatInt(value, isForScalar);

         /// <summary>
         /// Formats the display value of a boolean. The value is converted to a lowercase string ("true" or "false") for display purposes.
         /// </summary>
         /// <param name="value">Value to be displayed.</param>
         /// <param name="isForScalar">If true, the value is being formatted for a scalar context.</param>
         /// <returns>String value to be displayed.</returns>
         public virtual string VisitDisplayValue(ushort value, bool isForScalar) => FormatInt(value, isForScalar);

         /// <summary>
         /// Formats the display value of a boolean. The value is converted to a lowercase string ("true" or "false") for display purposes.
         /// </summary>
         /// <param name="value">Value to be displayed.</param>
         /// <param name="isForScalar">If true, the value is being formatted for a scalar context.</param>
         /// <returns>String value to be displayed.</returns>
         public virtual string VisitDisplayValue(uint value, bool isForScalar) => FormatInt(value, isForScalar);

         /// <summary>
         /// Formats the display value of a boolean. The value is converted to a lowercase string ("true" or "false") for display purposes.
         /// </summary>
         /// <param name="value">Value to be displayed.</param>
         /// <param name="isForScalar">If true, the value is being formatted for a scalar context.</param>
         /// <returns>String value to be displayed.</returns>
         public virtual string VisitDisplayValue(ulong value, bool isForScalar) => FormatInt(value, isForScalar);

         /// <summary>
         /// Formats the display value of a boolean. The value is converted to a lowercase string ("true" or "false") for display purposes.
         /// </summary>
         /// <param name="value">Value to be displayed.</param>
         /// <param name="isForScalar">If true, the value is being formatted for a scalar context.</param>
         /// <returns>String value to be displayed.</returns>
         public virtual string VisitDisplayValue(float value, bool isForScalar) => FormatNumeric(value, isForScalar);

         /// <summary>
         /// Formats the display value of a boolean. The value is converted to a lowercase string ("true" or "false") for display purposes.
         /// </summary>
         /// <param name="value">Value to be displayed.</param>
         /// <param name="isForScalar">If true, the value is being formatted for a scalar context.</param>
         /// <returns>String value to be displayed.</returns>
         public virtual string VisitDisplayValue(double value, bool isForScalar) => FormatNumeric(value, isForScalar);

         /// <summary>
         /// Formats the display value of a boolean. The value is converted to a lowercase string ("true" or "false") for display purposes.
         /// </summary>
         /// <param name="value">Value to be displayed.</param>
         /// <param name="isForScalar">If true, the value is being formatted for a scalar context.</param>
         /// <returns>String value to be displayed.</returns>
         public virtual string VisitDisplayValue(LongDouble value, bool isForScalar) => FormatNumeric(value, isForScalar);

         /// <summary>
         /// Formats the display value of a boolean. The value is converted to a lowercase string ("true" or "false") for display purposes.
         /// </summary>
         /// <param name="value">Value to be displayed.</param>
         /// <param name="isForScalar">If true, the value is being formatted for a scalar context.</param>
         /// <returns>String value to be displayed.</returns>
         public virtual string VisitDisplayValue(ComplexInt8 value, bool isForScalar) => FormatNumeric(value, isForScalar);

         /// <summary>
         /// Formats the display value of a boolean. The value is converted to a lowercase string ("true" or "false") for display purposes.
         /// </summary>
         /// <param name="value">Value to be displayed.</param>
         /// <param name="isForScalar">If true, the value is being formatted for a scalar context.</param>
         /// <returns>String value to be displayed.</returns>
         public virtual string VisitDisplayValue(ComplexInt16 value, bool isForScalar) => FormatNumeric(value, isForScalar);

         /// <summary>
         /// Formats the display value of a boolean. The value is converted to a lowercase string ("true" or "false") for display purposes.
         /// </summary>
         /// <param name="value">Value to be displayed.</param>
         /// <param name="isForScalar">If true, the value is being formatted for a scalar context.</param>
         /// <returns>String value to be displayed.</returns>
         public virtual string VisitDisplayValue(ComplexInt32 value, bool isForScalar) => FormatNumeric(value, isForScalar);

         /// <summary>
         /// Formats the display value of a boolean. The value is converted to a lowercase string ("true" or "false") for display purposes.
         /// </summary>
         /// <param name="value">Value to be displayed.</param>
         /// <param name="isForScalar">If true, the value is being formatted for a scalar context.</param>
         /// <returns>String value to be displayed.</returns>
         public virtual string VisitDisplayValue(ComplexInt64 value, bool isForScalar) => FormatNumeric(value, isForScalar);

         /// <summary>
         /// Formats the display value of a boolean. The value is converted to a lowercase string ("true" or "false") for display purposes.
         /// </summary>
         /// <param name="value">Value to be displayed.</param>
         /// <param name="isForScalar">If true, the value is being formatted for a scalar context.</param>
         /// <returns>String value to be displayed.</returns>
         public virtual string VisitDisplayValue(ComplexUint8 value, bool isForScalar) => FormatNumeric(value, isForScalar);

         /// <summary>
         /// Formats the display value of a boolean. The value is converted to a lowercase string ("true" or "false") for display purposes.
         /// </summary>
         /// <param name="value">Value to be displayed.</param>
         /// <param name="isForScalar">If true, the value is being formatted for a scalar context.</param>
         /// <returns>String value to be displayed.</returns>
         public virtual string VisitDisplayValue(ComplexUint16 value, bool isForScalar) => FormatNumeric(value, isForScalar);

         /// <summary>
         /// Formats the display value of a boolean. The value is converted to a lowercase string ("true" or "false") for display purposes.
         /// </summary>
         /// <param name="value">Value to be displayed.</param>
         /// <param name="isForScalar">If true, the value is being formatted for a scalar context.</param>
         /// <returns>String value to be displayed.</returns>
         public virtual string VisitDisplayValue(ComplexUint32 value, bool isForScalar) => FormatNumeric(value, isForScalar);

         /// <summary>
         /// Formats the display value of a boolean. The value is converted to a lowercase string ("true" or "false") for display purposes.
         /// </summary>
         /// <param name="value">Value to be displayed.</param>
         /// <param name="isForScalar">If true, the value is being formatted for a scalar context.</param>
         /// <returns>String value to be displayed.</returns>
         public virtual string VisitDisplayValue(ComplexUint64 value, bool isForScalar) => FormatNumeric(value, isForScalar);

         /// <summary>
         /// Formats the display value of a boolean. The value is converted to a lowercase string ("true" or "false") for display purposes.
         /// </summary>
         /// <param name="value">Value to be displayed.</param>
         /// <param name="isForScalar">If true, the value is being formatted for a scalar context.</param>
         /// <returns>String value to be displayed.</returns>
         public virtual string VisitDisplayValue(ComplexFloat value, bool isForScalar) => FormatNumeric(value, isForScalar);

         /// <summary>
         /// Formats the display value of a boolean. The value is converted to a lowercase string ("true" or "false") for display purposes.
         /// </summary>
         /// <param name="value">Value to be displayed.</param>
         /// <param name="isForScalar">If true, the value is being formatted for a scalar context.</param>
         /// <returns>String value to be displayed.</returns>
         public virtual string VisitDisplayValue(ComplexDouble value, bool isForScalar) => FormatNumeric(value, isForScalar);

         /// <summary>
         /// Formats the display value of a boolean. The value is converted to a lowercase string ("true" or "false") for display purposes.
         /// </summary>
         /// <param name="value">Value to be displayed.</param>
         /// <param name="isForScalar">If true, the value is being formatted for a scalar context.</param>
         /// <returns>String value to be displayed.</returns>
         public virtual string VisitDisplayValue(ComplexLongDouble value, bool isForScalar) => FormatNumeric(value, isForScalar);

         /// <summary>
         /// Formats the display value of a boolean. The value is converted to a lowercase string ("true" or "false") for display purposes.
         /// </summary>
         /// <param name="value">Value to be displayed.</param>
         /// <param name="isForScalar">If true, the value is being formatted for a scalar context.</param>
         /// <returns>String value to be displayed.</returns>
         public virtual string VisitDisplayValue(IntPtr value, bool isForScalar) => $"0x{((UniversalInt)value).Hex}";

         /// <summary>
         /// Formats an integer value using the UniversalInt representation.
         /// </summary>
         /// <param name="valueType">The integer value to format.</param>
         /// <returns>A string representation of the integer value.</returns>
         public virtual string FormatInt(ValueType? valueType, bool isForScalar) => 
            isForScalar ? 
               ((UniversalInt)(dynamic)(valueType ?? throw new Crash())).Resume : 
               (valueType ?? throw new Crash()).ToString() ?? throw new Crash();

         /// <summary>
         /// Formats a numeric value using the UniversalNumeric representation.
         /// </summary>
         /// <param name="floatValue">The numeric value to format.</param>
         /// <param name="isForScalar">If true, the value is being formatted for a scalar context.</param>
         /// <returns>A string representation of the numeric value.</returns>
         public virtual string FormatNumeric(ValueType? floatValue, bool isForScalar)
         {
            var un = (UniversalNumeric)(dynamic)(floatValue ?? throw new Crash());

            return isForScalar ? un.Resume : un.DisplayValue;
         }
      }

      protected virtual FormatVisitor myMakeFormatVisitor() => new FormatVisitor(myMakeBuiltInVisitor());

      protected virtual BuiltInVisitor myMakeBuiltInVisitor() => new BuiltInVisitor();


      public override string Format(RtmObj runTimeObj) => myMakeFormatVisitor().Visit((dynamic)runTimeObj);
   }
}
