using System.Text;

namespace UsageExample01
{
    internal class Program
    {
        static string definition = """
        interface InterfaceA {
            void AMethod();
            int AProperty { get; set; }
            event EventHandler AnEvent;
        }

        interface InterfaceB {
            void AMethod();
            int AProperty { get; set; }
            event EventHandler AnEvent;
        }

        interface InterfaceC : InterfaceA, InterfaceB {
            // This interface inherits from both InterfaceA and InterfaceB.
            // It can have additional members or override existing ones.
        }
        """;

        static void Main(string[] args)
        {
            int loopMax = 2 ^ 2;
            for (int i = 0; i <= loopMax; ++i)
            {
                bool shouldImplplementExplicitly = (i % 2) == 0;
                bool shouldImplementImplicitly = ((i >> 1) % 2) == 0;

                StringBuilder codeSb = new StringBuilder();
                codeSb.AppendLine(definition);

                if (shouldImplementImplicitly) { 
                string code = definition + """
                    {}
                    """;
            }
        }
    }
}
