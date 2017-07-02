using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spectrum.Plugins.ServerMod.CmdSettings
{
    abstract class CmdSettingDouble : CmdSetting
    {
        public override string UsageParameters { get; } = "<number>";
        public virtual double LowerBound { get { return double.MinValue; } }
        public virtual double UpperBound { get { return double.MaxValue; } }
        UpdateResult CheckBound(double num)
        {
            var BoundInvalid =
                (UpperBound != double.MaxValue && num > UpperBound)
                || (LowerBound != double.MinValue && num < LowerBound);
            if (BoundInvalid)
            {
                string Message;
                if (LowerBound != double.MinValue && UpperBound != double.MaxValue)
                    Message = $"Invalid number. Number should be between {LowerBound} and {UpperBound}";
                else if (LowerBound != double.MinValue)
                    Message = $"Invalid number. Number should be above {LowerBound}";
                else
                    Message = $"Invalid number. Number should be below {UpperBound}";
                return new UpdateResult(false, this.Value, Message);
            }
            return new UpdateResult(true, num);
        }
        public override UpdateResult UpdateFromString(string input)
        {
            double num;
            if (double.TryParse(input.Trim(), out num))
                return CheckBound(num);
            else
                return new UpdateResult(false, this.Value, "Invalid number.");
        }
        public override UpdateResult UpdateFromObject(object input)
        {
            if (input.GetType() == typeof(double))
                return CheckBound((double)input);
            else if (input.GetType() == typeof(float) || input.GetType() == typeof(int))
                return CheckBound((double)input);
            else
                return new UpdateResult(false, Default, "Type should be a double (any number)");
        }
    }
}
