Shader "Custom/Diffuse Color" {
	Properties {
		_Color ("Color", Color) = (1, 1, 1, 0.5)
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200
		
		CGPROGRAM
		#pragma surface surf Lambert

		fixed4 _Color;
		struct Input {
			fixed4 color : COLOR;
		};

		void surf (Input IN, inout SurfaceOutput o) {
			o.Albedo = _Color.rgb;
		}
		ENDCG
	} 
	FallBack "Diffuse"
}
