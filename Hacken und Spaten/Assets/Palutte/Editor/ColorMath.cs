using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Palutte {
	public class ColorMath {

		//translated from easyrgb.com/en/math

		public static float DeltaECIE(Vector3 CIELabCol1, Vector3 CIELabCol2) {

			return Mathf.Sqrt(Mathf.Pow(CIELabCol1.x - CIELabCol2.x, 2f)
				+ Mathf.Pow(CIELabCol1.y - CIELabCol2.y, 2f)
				+ Mathf.Pow(CIELabCol1.z - CIELabCol2.z, 2f));
		}

		public static float DeltaECIE94(Vector3 CIELabCol1, Vector3 CIELabCol2) {
			float xC1 = Mathf.Sqrt(Mathf.Pow(CIELabCol1.y, 2) + Mathf.Pow(CIELabCol1.z, 2));
			float xC2 = Mathf.Sqrt(Mathf.Pow(CIELabCol2.y, 2) + Mathf.Pow(CIELabCol2.z, 2));
			float xDL = CIELabCol2.x - CIELabCol1.x;
			float xDC = xC2 - xC1;
			float xDE = Mathf.Sqrt(((CIELabCol1.x - CIELabCol2.x) * (CIELabCol1.x - CIELabCol2.x))
				+ ((CIELabCol1.y - CIELabCol2.y) * (CIELabCol1.y - CIELabCol2.y))
				+ ((CIELabCol1.z - CIELabCol2.z) * (CIELabCol1.z - CIELabCol2.z)));

			float xDH = (xDE * xDE) - (xDL * xDL) - (xDC * xDC);

			if (xDH > 0f) {
				xDH = Mathf.Sqrt(xDH);
			} else {
				xDH = 0f;
			}

			float xSC = 1f + (0.045f * xC1);
			float xSH = 1f + (0.015f * xC1);

			float weightL = 1f;//wish I could find examples of what these should be
			float weightC = 1f;
			float weightH = 1f;

			xDL /= weightL;
			xDC /= weightC * xSC;
			xDH /= weightH * xSH;

			return Mathf.Sqrt(Mathf.Pow(xDL, 2) + Mathf.Pow(xDC, 2) + Mathf.Pow(xDH, 2));
		}

		public static Vector3 RGBtoCIELab(Color c) {
			return XYZtoCIELab(RGBtoXYZ(c));
		}

		public static Vector3 RGBtoXYZ(Color c) {
			float var_R = RGBtoXYZStepOne(c.r);
			float var_G = RGBtoXYZStepOne(c.g);
			float var_B = RGBtoXYZStepOne(c.b);

			Vector3 xyz = Vector3.zero;
			xyz.x = var_R * 0.4124f + var_G * 0.3576f + var_B * 0.1805f;
			xyz.y = var_R * 0.2126f + var_G * 0.7152f + var_B * 0.0722f;
			xyz.z = var_R * 0.0193f + var_G * 0.1192f + var_B * 0.9505f;

			return xyz;
		}
		static float RGBtoXYZStepOne(float v) {
			if (v > 0.04045f) {
				v = Mathf.Pow((v + 0.055f) / 1.055f, 2.4f);
			} else {
				v /= 12.92f;
			}
			v *= 100f;
			return v;
		}

		public static Vector3 XYZtoCIELab(Vector3 xyz) {
			Vector3 varXYZ = Vector3.zero;
			varXYZ.x = xyz.x / 98f; //no idea if these values are good,
			varXYZ.y = xyz.y / 100f; //honestly I don't understand any of this math
			varXYZ.z = xyz.z / 85f; //what space are the colours even in rn?

			varXYZ.x = XYZtoCIELabStepOne(varXYZ.x);
			varXYZ.y = XYZtoCIELabStepOne(varXYZ.y);
			varXYZ.z = XYZtoCIELabStepOne(varXYZ.z);

			Vector3 CIE_Lab = Vector3.zero;
			CIE_Lab.x = (116f * varXYZ.y) - 16f;
			CIE_Lab.y = 500f * (varXYZ.x - varXYZ.y);
			CIE_Lab.z = 200f * (varXYZ.y - varXYZ.z);

			return CIE_Lab;
		}
		static float XYZtoCIELabStepOne(float v) {
			if (v > 0.008856f) {
				v = Mathf.Pow(v, 1f / 3f);
			} else {
				v = (7.787f * v) + (16f / 116f);
			}
			return v;
		}


	}
}