public class vector3{
        public double x;
        public double y;
        public double z;
        public vector3(){
            //Empty Case
        }
        public vector3(double x, double y, double z){
            this.x = x;
            this.y = y;
            this.z = z;
        }
        public static vector3 add(vector3 first, vector3 second){
            return new vector3(
                first.x + second.x,
                first.y + second.y,
                first.z + second.z
            );
        }
        public static vector3 subtract(vector3 first, vector3 second){
            return new vector3(
                first.x - second.x,
                first.y - second.y,
                first.z - second.z
            );
        }
        public static vector3 scale(double scale, vector3 first){
            return new(
                first.x * scale,
                first.y * scale,
                first.z * scale
            );
        }
        public static double magnitude(vector3 first){
            return Math.Sqrt(
                first.x * first.x +
                first.y * first.y +
                first.z * first.z
            );
        }
        public static vector3 normalize(vector3 first){
            return new vector3(
                first.x / vector3.magnitude(first),
                first.y / vector3.magnitude(first),
                first.z / vector3.magnitude(first)
            );
            
        }
        public void scale(double scale){
            x = x * scale;
            y = y * scale;
            z = z * scale;
        }
        public void add(vector3 first){
            x += first.x;
            y += first.y;
            z += first.z;
        }
        public void subtract(vector3 first){
            x -= first.x;
            y -= first.y;
            z -= first.z;
        }
        public double magnitude(){
            return Math.Sqrt(
                x * x +
                y * y +
                z * z
            );
        }
        public void normalize(){
            x = x / magnitude();
            y = y / magnitude();
            z = z / magnitude();
        }
    }