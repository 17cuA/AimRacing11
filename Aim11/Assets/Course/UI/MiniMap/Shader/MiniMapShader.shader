// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)
/*====================================================*/
// ���e		�F�~�j�}�b�v�p�̃}�X�N����
// �t�@�C��	�FMiniMapShader.shader
//
// Copyright (C) ���J�@�i All Rights Reserved.
/*----------------------------------------------------*/
//�k�X�V�����l
// 2018/07/09 �V�K�쐬 �������ŕ\�������悤��
// 2018/07/11 ���߃O���f�[�V�����}�X�N����
/*====================================================*/

Shader "UI/MiniMap"
{
	Properties
	{
		[PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
	_Color("Tint", Color) = (1,1,1,1)

		_StencilComp("Stencil Comparison", Float) = 8
		_Stencil("Stencil ID", Float) = 0
		_StencilOp("Stencil Operation", Float) = 0
		_StencilWriteMask("Stencil Write Mask", Float) = 255
		_StencilReadMask("Stencil Read Mask", Float) = 255

		_ColorMask("Color Mask", Float) = 15

		[Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip("Use Alpha Clip", Float) = 0

		_MaskTex("MaskTexture",2D) = "white"{}	// �~�j�}�b�v�̕`��͈͂��}�X�N�Ő�������
	}

		SubShader
	{
		Tags
	{
		"Queue" = "Transparent"
		"IgnoreProjector" = "True"
		"RenderType" = "Transparent"
		"PreviewType" = "Plane"
		"CanUseSpriteAtlas" = "True"
	}

		Stencil
	{
		Ref[_Stencil]
		Comp[_StencilComp]
		Pass[_StencilOp]
		ReadMask[_StencilReadMask]
		WriteMask[_StencilWriteMask]
	}

		Cull Off
		Lighting Off
		ZWrite Off
		ZTest[unity_GUIZTestMode]
		Blend SrcAlpha OneMinusSrcAlpha
		ColorMask[_ColorMask]

		Pass
	{
		Name "Default"
		CGPROGRAM
#pragma vertex vert
#pragma fragment frag
#pragma target 2.0

#include "UnityCG.cginc"
#include "UnityUI.cginc"

#pragma multi_compile __ UNITY_UI_CLIP_RECT
#pragma multi_compile __ UNITY_UI_ALPHACLIP

		struct appdata_t
	{
		float4 vertex   : POSITION;
		float4 color    : COLOR;
		float2 texcoord : TEXCOORD0;
		UNITY_VERTEX_INPUT_INSTANCE_ID
	};

	struct v2f
	{
		float4 vertex   : SV_POSITION;
		fixed4 color : COLOR;
		float2 texcoord  : TEXCOORD0;
		float4 worldPosition : TEXCOORD1;
		UNITY_VERTEX_OUTPUT_STEREO
	};

	sampler2D _MainTex;
	fixed4 _Color;
	fixed4 _TextureSampleAdd;
	float4 _ClipRect;
	float4 _MainTex_ST;

	v2f vert(appdata_t v)
	{
		v2f OUT;
		UNITY_SETUP_INSTANCE_ID(v);
		UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
		OUT.worldPosition = v.vertex;
		OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);

		OUT.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);

		OUT.color = v.color * _Color;
		return OUT;
	}

	sampler2D _MaskTex;	//�}�X�N�摜

	fixed4 frag(v2f IN) : SV_Target
	{
		half4 color = (tex2D(_MainTex, IN.texcoord) + _TextureSampleAdd) * IN.color;


		float2 maskTexcoord = IN.texcoord;
		// �䗦�����킹���~�ɂ��� �F *(16.0/9.0)
		// ��������C������ �F - (16.0 / 9.0) / 2.0 - 0.5
		maskTexcoord.x = IN.texcoord.x * (16.0 / 9.0) - (16.0 / 9.0) / 2.0 - 0.5;


		half maskAlpha = tex2D(_MaskTex, maskTexcoord).a;	//�}�X�N

															//�R�[�X���}�X�N�̃A���t�@�l�ɍ��킹��(�͈͐����A�`��O���f)
		color.a *= maskAlpha;

		//���𔒂��\��
		color.rgb = _Color;

		return color;
	}
		ENDCG
	}
	}
}