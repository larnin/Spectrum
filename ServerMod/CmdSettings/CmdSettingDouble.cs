using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spectrum.Plugins.ServerMod.CmdSettings
{
    abstract class CmdSettingDouble : CmdSetting<double>
    {
        public override string UsageParameters { get; } = "<number>";
        public virtual double LowerBound { get { return double.MinValue; } }
        public virtual double UpperBound { get { return double.MaxValue; } }
        UpdateResult<double> CheckBound(double num)
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
                return new UpdateResult<double>(false, this.Value, Message);
            }
            return new UpdateResult<double>(true, num);
        }
        public override UpdateResult<double> UpdateFromString(string input)
        {
            double num;
            if (double.TryParse(input.Trim(), out num))
                return CheckBound(num);
            else
                return new UpdateResult<double>(false, this.Value, "Invalid number.");
        }
        public override UpdateResult<double> UpdateFromObject(object input)
        {
            if (input is double)
                return CheckBound((double)input);
            else if (input is float || input is int)
                return CheckBound((double)input);
            else
                return new UpdateResult<double>(false, Default, "Type should be a double (any number)");
        }
    }
}
