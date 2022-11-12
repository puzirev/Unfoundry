using System;

namespace Unfoundry
{
    public class CancellableEventArgs : EventArgs
    {
        public bool Cancel { get; set; } = false;

        public override bool Equals(object obj) => obj is CancellableEventArgs args && Cancel == args.Cancel;
        public override int GetHashCode() => -547181076 + Cancel.GetHashCode();
        public override string ToString() => Cancel.ToString();
    }
}