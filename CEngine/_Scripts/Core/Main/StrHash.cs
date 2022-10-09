namespace CYM
{
    public class StrHash
    {
        
        public string String { get; private set; }
        public int HashCode { get; private set; }
        public StrHash(string str)
        {
            String = str;
            HashCode = String.GetHashCode();
        }

        public override int GetHashCode()
        {
            return HashCode;
        }
        public override bool Equals(object obj)
        {
            StrHash p = obj as StrHash;
            return p.HashCode == HashCode;
        }
    }
}