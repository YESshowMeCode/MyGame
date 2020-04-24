using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Text.RegularExpressions;
using UnityEngine.Serialization;

namespace UGUIExtend
{
	[RequireComponent(typeof(RectTransform))]
	[RequireComponent(typeof(CanvasRenderer))]
	[AddComponentMenu("UI/OutLineText",12)]
	public class OutLineText : Text {
		
		protected virtual void LoadSprite(Image image, string path)
		{
			if(image.sprite == null || image.sprite.name != path)
			{
				image.sprite = Resources.Load<Sprite>(imagePathRoot + path);
			}
		}


		[Serializable]
		public class CharOffest
		{
			public Vector2 position = Vector2.zero;
			public float rotation = 0f;
			public Vector2 scale = Vector2.one;
		}

		[Serializable]
		public class InLineImage
		{
			public bool cull;
			public Image image;
			public 	InLineImage(Image image)
			{
				this.image = image;
				this.cull = false;
			}
		}

		private static readonly Regex s_Regex = new Regex(@"<image scr=(\S+?)(?: width=(\d*\.?\d+))?(?: height=(\d*\.?\d+))?/>",RegexOptions.Singleline);

        [SerializeField]
        private bool m_Visible = true;
		public bool visible
		{
			get{return m_Visible;}
			set
			{
				if(m_Visible != value)
				{
					m_Visible = value;
					UpdateVisible();
					if(m_Visible)
					{
						CanvasUpdateRegistry.RegisterCanvasElementForGraphicRebuild(this);
					}
				}
			}
		}

	    [SerializeField]
		public string imagePathRoot = "";

		public enum TextEffextType
		{
			NONE,
			SHADOW,
			OUTLINE4,
			OUTLINE8,
			MATERIAL,
		}

        [SerializeField]
        private TextEffextType m_EffectType = TextEffextType.NONE;

		public TextEffextType effextType
		{
			get{return m_EffectType;}
			set
			{
				if(m_EffectType != value)
				{
					m_EffectType = value;
					SetVerticesDirty();
				}
			}
		}

        [SerializeField]
        private Color32 m_EffectColor = new Color32(0,0,0,128);

		public Color32 effectColor
		{
			get{return m_EffectColor;}
			set
			{
				if(!Equals(m_EffectColor,value))
				{
					m_EffectColor = value;
					SetVerticesDirty();
				}
			}
		}


        [SerializeField]
        private Vector2 m_EffectDistance = new Vector2(1f,-1f);
		public Vector2 effectDistance
		{
			get{return m_EffectDistance;}
			set
			{
				if(m_EffectDistance != value)
				{
					m_EffectDistance = value;
					SetVerticesDirty();
				}
			}
		}

        [SerializeField]
        private bool m_UseGraphicAlpha = false;
		public bool useGraphicAlpha
		{
			get{return m_UseGraphicAlpha;}
			set
			{
				if(m_UseGraphicAlpha != value)
				{
					m_UseGraphicAlpha = value;
					SetVerticesDirty();
				}
			}
		}

        [SerializeField]
        private bool m_EnabledGradient = false;
		public bool enabledGradient
		{
			get{return m_EnabledGradient;}
			set
			{
				if(m_EnabledGradient != value)
				{
					m_EnabledGradient = value;
					SetVerticesDirty();
				}
			}
		}

        [SerializeField]
        private Color m_GradientColor = Color.white;
		public Color gradientColor	
		{
			get{return m_GradientColor;}
			set
			{
				if(m_GradientColor != value)
				{
					m_GradientColor = value;
					SetVerticesDirty();
				}
			}
		}

        [SerializeField]
        public List<CharOffest> charOffests = new List<CharOffest>();
		public void SetCharOffest(int index,Vector3 position)
		{
			SeekToCharOffestIndex(index);
			this.charOffests[index].position = position;
			SetVerticesDirty();
		}

		public void SetChatOffest(int index , Vector3 position, float rotation)
		{
			SetCharOffest(index,position);
			this.charOffests[index].rotation = rotation;
			SetVerticesDirty();
		}

		public void SetChatOffest(int index , Vector3 position, float rotation ,Vector3 scale)
		{
			SetChatOffest(index,position,rotation);
			this.charOffests[index].scale = scale;
			SetVerticesDirty();
		}


		private void SeekToCharOffestIndex(int index)
		{
			for(int i=charOffests.Count; i<=index ; i++)
			{
				this.charOffests.Add(null);
			}

			if(this.charOffests[index] == null)
			{
				this.charOffests[index] = new CharOffest();
			}
		}

		private string m_OldNoFilterText;
		private string m_FilterText;
		private bool m_OldSupportRichText;

		public override void SetVerticesDirty()
		{
			m_OldNoFilterText = m_Text;
			m_OldSupportRichText = supportRichText;
            m_FilterText = FilterRichText(m_Text);

        }


		private void SetInlineImageCull(int index ,bool cull)
		{
			InLineImage item = inLineImages[index];
			if(item.image == null)
			{
				return;
			}

			item.cull = cull;
			if(cull != item.image.canvasRenderer.cull)
			{
				item.image.canvasRenderer.cull = cull;
				if(!cull)
				{
					item.image.Rebuild(CanvasUpdate.PreRender);
				}
			}
		}		

		private string FilterRichText(string text)
		{
			inLineImages.Clear();
			int i=0;
			if(supportRichText)
			{
				Match match;
				do
				{
					match = s_Regex.Match(text);
					if(match.Success)
					{
						inLineCharindex.Add(match.Index);

						string src = match.Groups[1].Value;
						float width = string.IsNullOrEmpty(match.Groups[2].Value)?float.NaN: float.Parse(match.Groups[2].Value);
						float height = string.IsNullOrEmpty(match.Groups[3].Value)?float.NaN: float.Parse(match.Groups[3].Value);
						string newText = "";
						Image img = null;

						if(i<inLineImages.Count || inLineImages[i].image == null)
						{
							img = new GameObject("InlineImage" + (i + 1).ToString()).AddComponent<Image>();
							img.transform.SetParent(this.transform,false);
							img.raycastTarget = false;

							if(i<inLineImages.Count)
							{
								inLineImages[i].image = img;
								inLineImages[i].cull = false;
							}
							else
							{
								inLineImages.Add(new InLineImage(img));
							}
						}

						LoadSprite(img,match.Groups[1].Value);
						Sprite spr = img.sprite;

						if(spr != null)
						{
							if(float.IsNaN(height) && float.IsNaN(width))
							{
								height = fontSize;
								width = height / spr.rect.height * spr.rect.width;
							}
							else if(float.IsNaN(width))
							{
								width = height / spr.rect.height * spr.rect.width;
							}
							else if(float.IsNaN(height))
							{
								height = width / spr.rect.height * spr.rect.width;
							}
							img.rectTransform.sizeDelta = new Vector2(width,height);
							newText = "<quad size=" + height.ToString() + "width=" + width.ToString() + "height" + height.ToString() + "/>";
						}

						text = text.Substring(0,match.Index) + newText + text.Substring(match.Index + match.Length);
						i++;
					}
				} while (match.Success);
			}
			int c = inLineImages.Count - 1;
			while(i <= c)
			{
				if(inLineImages[c].image != null)
				{
					inLineImages[c].image.sprite = null;
					SetInlineImageCull(c,true);
				}
#if UNITY_EDITOR
				else
				{
					inLineImages.RemoveAt(c);
				}
#endif
				c--;
			}
			return text;
		}


		[SerializeField]
		private List<InLineImage> inLineImages = new List<InLineImage>();
		private List<int> inLineCharindex = new List<int>();


		protected override void OnEnable()
		{
			base.OnEnable();
			if(inLineImages != null)
			{
				int count = inLineImages.Count;
				for(int i = 0;i<count;i++)
				{
					if(inLineImages[i].image != null)
					{
						inLineImages[i].image.enabled = true;
					}
				}
			}
			UpdateVisible();
		}

		protected override void OnDisable()
		{
			base.OnDisable();
			if(inLineImages != null)
			{
				int count = inLineImages.Count;
				for(int i = 0;i<count;i++)
				{
					if(inLineImages[i].image != null)
					{
						inLineImages[i].image.enabled = false;
					}
				}
			}
		}

		protected virtual void UpdateVisible()
		{
			this.canvasRenderer.cull = !m_Visible;
			if(inLineImages != null)
			{
				int count = inLineImages.Count;
				for(int i=0; i<count;i++)
				{
					InLineImage item = inLineImages[i];
					if(item.image != null)
					{
						bool cull = !m_Visible||item.cull;
						if(cull != item.image.canvasRenderer.cull)
						{
							item.image.canvasRenderer.cull = cull;
							if(!cull)
							{
								//不加这个重建在隐藏状态时有变化不会更新
								CanvasUpdateRegistry.RegisterCanvasElementForGraphicRebuild(item.image);
							}
						}
					}
				}
			}
		}

		public void ClearUnUsedInlineImage()
		{
			if(inLineImages != null)
			{
				int needCount = inLineCharindex.Count;
				int count = inLineImages.Count;

				for(int i = count - 1;i>0;i--)
				{
					InLineImage item = inLineImages[i];
					if(item.image != null && i>= needCount)
					{
						if(Application.isPlaying)
						{
							GameObject.Destroy(item.image.gameObject);
						}
						else
						{
							GameObject.DestroyImmediate(item.image.gameObject);
						}
						item.image = null;
					}

					if(item.image == null)
					{
						inLineImages.RemoveAt(i);
					}
				}
			}

			SetVerticesDirty();
			m_FilterText = FilterRichText(m_Text);
		}


		readonly UIVertex[] m_TempVerts = new UIVertex[4];
		readonly UIVertex[] m_TempEffectVerts = new UIVertex[4];

		protected override void OnPopulateMesh(VertexHelper toFill)
		{
			if(font == null)
				return;

			m_DisableFontTextureRebuiltCallback = true;

			Vector2 extents = rectTransform.rect.size;

			var setting = GetGenerationSettings(extents);
			cachedTextGenerator.PopulateWithErrors(m_FilterText,setting,gameObject);
			OnPopulateVertsPosition(toFill);

			m_DisableFontTextureRebuiltCallback = false;	
		}

		protected virtual void OnPopulateVertsPosition(VertexHelper toFill)
		{
			IList<UIVertex> verts = cachedTextGenerator.verts;
			float unitsPerPixel = 1 / pixelsPerUnit;
			
			int vertCount = verts.Count - 4;

			Vector2 roundingOffest = new Vector2(verts[0].position.x, verts[0].position.y) * unitsPerPixel;
			roundingOffest = PixelAdjustPoint(roundingOffest) - roundingOffest;
			toFill.Clear();
			bool needOffest = roundingOffest != Vector2.zero;

			//处理图文混排的图片
			int count = this.inLineCharindex.Count;
			for(int i = 0;i < count; i++)
			{
				int index = inLineCharindex[i];
				if(index*4 + 3 < vertCount)
				{
					if(i < inLineImages.Count && inLineImages[i].image != null)
					{
						Vector2 topLeft = verts[index*4 + 1].position;
						Vector2 bottomRight = verts[index*4 + 1].position;
						Vector2 center = new Vector2((topLeft.x + bottomRight.x) * 0.5f, topLeft.y + (bottomRight.y - topLeft.y)*0.6f);
						topLeft.y += (bottomRight.y - topLeft.y)*0.5f;
						var img = inLineImages[i].image;
						img.transform.localPosition = center*unitsPerPixel;
						SetInlineImageCull(i,false);
						UIVertex newVertex = UIVertex.simpleVert;
						newVertex.position = center;
						verts[index*4] = newVertex;
						verts[index*4 + 1] = newVertex;
						verts[index*4 + 2] = newVertex;
						verts[index*4 + 3] = newVertex;
					}
				}
				else
				{
					if(i < inLineImages.Count)
					{
						SetInlineImageCull(i,true);
					}
				}
			}
		

			int charIndex = 0;
			bool hasCharOffests = charOffests!= null && charOffests.Count>0;

			float bottomY = float.MaxValue;
			float topY = float.MaxValue;
			if(m_EnabledGradient)
			{
				for(int i = 0; i < vertCount; i+=2)
				{
					float y = verts[i].position.y;
					if(y > topY)
					{
						topY = y;
					}
					else if(y < bottomY)
					{
						bottomY = y;
					}
				}
			}

			for(int i= 0 ; i < vertCount; i+=4)
			{
				if(verts[i].position == verts[i + 1].position)
					continue;
				
				for(int j = 0; j < 4; ++j)
				{
					m_TempVerts[i] = verts[i + j];
					m_TempVerts[j].position *= unitsPerPixel;
					if(needOffest)
					{
						m_TempVerts[j].position.x += roundingOffest.x;
						m_TempVerts[j].position.y += roundingOffest.y;
					}
				}

				//文字位移
				float cosRotate = 0f;
				float sinRotate = 0f;
				if(hasCharOffests && charIndex < charOffests.Count)
				{
					CharOffest charOffest = charOffests[charIndex];
					bool needScale = charOffest.scale != Vector2.one;
					if(charOffest.rotation != 0f)
					{
						cosRotate = Mathf.Cos(charOffest.rotation);
						sinRotate = Mathf.Sin(charOffest.rotation);
					}

					if(charOffest != null)
					{
						Vector2 center = (m_TempVerts[0].position + m_TempVerts[2].position)/2;
						for(int j = 0;j < 4;++j)
						{
							if(needScale)
							{
								m_TempVerts[j].position.x = center.x + (m_TempVerts[j].position.x - center.x) * charOffest.scale.x;
								m_TempVerts[j].position.y = center.y + (m_TempVerts[j].position.y - center.y) * charOffest.scale.y;
							}
							if(charOffest.rotation != 0f)
							{
								float dx = m_TempVerts[j].position.x - center.x;
								float dy = m_TempVerts[j].position.y - center.y;
								m_TempVerts[j].position.x = center.x + dx*cosRotate - dy*sinRotate;
								m_TempVerts[j].position.y = center.y + dx*sinRotate - dy*cosRotate;
							}
							m_TempVerts[j].position.x += charOffest.position.x;
							m_TempVerts[j].position.y += charOffest.position.y;
						}
					}
					
				}

				if(m_EnabledGradient)
				{
					ApplyGradientColor(m_TempVerts,color,m_GradientColor,topY,bottomY);
				}


				//阴影与描边
				if(m_EffectType != TextEffextType.NONE)
				{
					if(m_EffectType == TextEffextType.MATERIAL)
					{
						Vector2 bottomLeft = m_TempVerts[0].uv0;
						Vector2 topRight = m_TempVerts[2].uv0;

						if(bottomLeft.x > topRight.x)
						{
							bottomLeft = m_TempVerts[2].uv0;
							topRight = m_TempVerts[0].uv0;
						}
						Vector4 uvBounds = new Vector4(bottomLeft.x,bottomLeft.y,topRight.x,topRight.y);
						m_TempVerts[0].tangent = uvBounds;
						m_TempVerts[1].tangent = uvBounds;
						m_TempVerts[2].tangent = uvBounds;
						m_TempVerts[3].tangent = uvBounds;
					}
					else
					{
						m_TempVerts.CopyTo(m_TempEffectVerts,0);
						ApplyColor(m_TempEffectVerts,m_EffectColor);
						ApplyOffestX(m_TempEffectVerts,m_EffectDistance.x);
						ApplyOffestY(m_TempEffectVerts,m_EffectDistance.y);
						toFill.AddUIVertexQuad(m_TempEffectVerts);

						if(m_EffectType != TextEffextType.SHADOW)
						{
							ApplyOffestY(m_TempEffectVerts,-m_EffectDistance.y - m_EffectDistance.y);
							toFill.AddUIVertexQuad(m_TempEffectVerts);
							ApplyOffestY(m_TempEffectVerts,-m_EffectDistance.x - m_EffectDistance.x);
							toFill.AddUIVertexQuad(m_TempEffectVerts);
							ApplyOffestY(m_TempEffectVerts,m_EffectDistance.y + m_EffectDistance.y);
							toFill.AddUIVertexQuad(m_TempEffectVerts);

							if(m_EffectType != TextEffextType.OUTLINE4)
							{
								const float sqrt2 = 1.414214f;
								ApplyOffestX(m_TempEffectVerts,m_EffectDistance.x);
								ApplyOffestY(m_TempEffectVerts,(sqrt2-1) * m_EffectDistance.y);
								toFill.AddUIVertexQuad(m_TempEffectVerts);
								ApplyOffestY(m_TempEffectVerts,-sqrt2 * 2 * m_EffectDistance.y);
								toFill.AddUIVertexQuad(m_TempEffectVerts);
								ApplyOffestY(m_TempEffectVerts,sqrt2 * m_EffectDistance.y);
								ApplyOffestX(m_TempEffectVerts,sqrt2 * m_EffectDistance.x);
								toFill.AddUIVertexQuad(m_TempEffectVerts);
								ApplyOffestX(m_TempEffectVerts,-sqrt2 * 2 * m_EffectDistance.x);
								toFill.AddUIVertexQuad(m_TempEffectVerts);
							}
							
						}


					}
				};
				toFill.AddUIVertexQuad(m_TempVerts);
				charIndex++;
			};

		}


		void ApplyColor(UIVertex[] vertexs,Color32 effectColor)
		{
			if(m_UseGraphicAlpha)
			{
				effectColor.a = (byte)(effectColor.a * vertexs[0].color.a / 255);
			}

			vertexs[0].color = effectColor;
			vertexs[1].color = effectColor;
			vertexs[2].color = effectColor;
			vertexs[3].color = effectColor;
		}


		void ApplyGradientColor(UIVertex[] vertexs , Color32 topColor, Color32 bottomColor, float topY, float bottomY)
		{
			if(m_UseGraphicAlpha)
			{
				topColor.a = (byte)(topColor.a * vertexs[0].color.a / 255);
				bottomColor.a = (byte)(bottomColor.a * vertexs[0].color.a / 255);
			}

			float uiElementheight = topY - bottomY;
			vertexs[0].color = vertexs[1].color = Color32.Lerp(bottomColor,topColor,(vertexs[0].position.y - bottomY) / uiElementheight);
			vertexs[2].color = vertexs[3].color = Color32.Lerp(bottomColor,topColor,(vertexs[2].position.y - bottomY) / uiElementheight);
		}


		void ApplyOffestX(UIVertex[] vertexs, float v)
		{
			vertexs[0].position.x += v;
			vertexs[1].position.x += v;
			vertexs[2].position.x += v;
			vertexs[3].position.x += v;
		}

		void ApplyOffestY(UIVertex[] vertexs, float v)
		{
			vertexs[0].position.y += v;
			vertexs[1].position.y += v;
			vertexs[2].position.y += v;
			vertexs[3].position.y += v;
		}

		protected readonly static Vector3[] fourCorners = new Vector3[4];

		private void OnDrawGizmosSelected()
		{
			Gizmos.color = Color.green;
			rectTransform.GetWorldCorners(fourCorners);
			for(int i = 0;i < 4;i++)
			{
				Gizmos.DrawLine(fourCorners[i],fourCorners[(i+1)%4]);
			}
		}

		protected override void OnValidate()
		{
			base.OnValidate();
			UpdateVisible();
		}
	}
}

