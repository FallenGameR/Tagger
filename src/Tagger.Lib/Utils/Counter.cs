using System;

namespace Tagger
{
    public class Counter
    {
        private DateTime last = DateTime.Now;
        private int counter = 1;
        private int max = 0;
        private string name;

        public Counter(string name)
        {
            this.name = name;
        }

        public void Next()
        {
            var now = DateTime.Now;
            var elapsed = now - last;

            if (elapsed.TotalSeconds > 1)
            {
                if (counter > max)
                {
                    max = counter;
                    Console.WriteLine("{0} max rate of calls per second = {1}", name, max);
                }
                last = now;
                counter = 1;
            }
            else
            {
                counter++;
            }
        }
    }
}
