using System;
using UnityEngine;

namespace Delaunay {
    public class Edge
    {
        public readonly Vertex a;
        public readonly Vertex b;

        float length = -1;
        public float Length
        {
            get {
                if ( length < 0 )
                {
                    float dx = a.x - b.x;
                    float dy = a.y - b.y;
                    length = Mathf.Sqrt(dx * dx + dy * dy);
                }
                return length; 
            }
        }

        // a < b 
        public Edge(Vertex a, Vertex b)
        {
           if(a == b )
            {
                Debug.Log("Same Vertex");
            }
            else if(a<b)
            {
                this.a = a;
                this.b = b;
            }
            else 
            {
                this.a = b;
                this.b = a;
            }
        }


        public static int LengthCompare( Edge x, Edge y )
        {
            float lx = x.length;
            float ly = y.length;

            // float 비교시 == 대신 Approximately (근사치) 사용
            if ( Mathf.Approximately(lx, ly) ) return 0;
            else if ( lx > ly ) return 1;
            else return -1;
        }


        /*****************************************
        *             Override Methods
        *****************************************/
        public override string ToString()
        {
            return "(" + a + ", " + b + ")";
        }

        public override bool Equals( object obj )
        {
            return obj is Edge e && a == e.a && b == e.b;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(a, b);
        }
       

    }
}

