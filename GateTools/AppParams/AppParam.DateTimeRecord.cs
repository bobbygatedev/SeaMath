using System;

namespace Gate.Tools.AppParams
{
   public abstract partial class AppParam
   {
      /// <summary>
      /// Represents a record that encapsulates date and time components, allowing for manipulation and retrieval of
      /// individual parts of a <see cref="DateTime"/> value.
      /// </summary>
      /// <remarks>This class provides properties for accessing and modifying the year, month, day, hour,
      /// minute,  second, millisecond, and <see cref="DateTimeKind"/> of a <see cref="DateTime"/> value.  The <see
      /// cref="DateTime"/> property allows for getting or setting the full date and time  as a single value, while the
      /// individual components can be accessed or modified separately.</remarks>
      public class DateTimeRecord : Record
      {
         public DateTimeRecord() { }
         
         public DateTimeRecord(string name, string? caption = null) { }

         public readonly Simple<int> Year = new Simple<int>();
         public readonly Simple<int> Month = new Simple<int>();
         public readonly Simple<int> Day = new Simple<int>();
         public readonly Simple<int> Hour = new Simple<int>();
         public readonly Simple<int> Minutes = new Simple<int>();
         public readonly Simple<int> Seconds = new Simple<int>();
         public readonly Simple<int> Milliseconds = new Simple<int>();
         private readonly Simple<string> myKind = new Simple<string>();

         public DateTimeKind Kind
         {
            get =>
               Enum.TryParse<DateTimeKind>(myKind.Value, out var kind) ? kind : DateTimeKind.Unspecified;

            set => myKind.Value = value.ToString();
         }

         public DateTime DateTime
         {
            get => new DateTime(
                  Year.Value,
                  Month.Value,
                  Day.Value,
                  Hour.Value,
                  Minutes.Value,
                  Seconds.Value,
                  Milliseconds.Value,
                  Kind);

            set
            {
               Year.Value = value.Year;
               Month.Value = value.Month;
               Day.Value = value.Day;
               Hour.Value = value.Hour;
               Minutes.Value = value.Minute;
               Seconds.Value = value.Second;
               Milliseconds.Value = value.Millisecond;
               Kind = value.Kind;
            }
         }
      }
   }
}
