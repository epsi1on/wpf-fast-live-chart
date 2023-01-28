namespace WpfFastCharting.Lib
{
    public class OneDTransformation
    {
        public double alpha, betta;

        public double Transform(double value)
        {
            return alpha * value + betta;
        }


        public OneDTransformation()
        {

        }

        public OneDTransformation(double alpha, double betta)
        {
            this.alpha = alpha;
            this.betta = betta;
        }

        public static OneDTransformation FromInOut(
            double in1, double in2, 
            double out1, double ou2)
        {
            //octave:
            //syms in1 in2 out1 ou2
            //inv([in1,1;in2,1])*[out1;ou2]


            var buf = new OneDTransformation();

            buf.alpha = -ou2 / (-in2 + in1) + out1 / (-in2 + in1);
            buf.betta = -in2 * out1 / (-in2 + in1) + ou2 * in1 / (-in2 + in1);

            return buf;
        }
    }
}