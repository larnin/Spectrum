using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spectrum.Plugins.ServerMod.CmdSettings
{
    abstract class UpdateResult
    {
        public bool Valid;
        public string Message = "";

        public abstract object NewValueTypeless { get; set; }
    }
    class UpdateResult<T> : UpdateResult
    {
        public T NewValue;
        public override object NewValueTypeless
        {
            get { return NewValue; }
            set { NewValue = (T)value; }
        }
        public UpdateResult(bool Valid, T NewValue)
        {
            this.Valid = Valid;
            this.NewValue = NewValue;
        }

        public UpdateResult(bool Valid, T NewValue, string Message)
        {
            this.Valid = Valid;
            this.NewValue = NewValue;
            this.Message = Message;
        }
    }
}
