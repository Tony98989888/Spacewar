using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

// This script controls a widget for a target tracking visor, including setting the position and style for 
// all of the widget components

namespace VSX.Vehicles{

	[RequireComponent(typeof(CanvasGroup))]
	public class DemoVisorWidget : MonoBehaviour, IVisorWidget {
	
		RectTransform thisRectTransform;
		GameObject thisGameObject;
	
		// For fading out many canvas objects at the same time
		CanvasGroup canvasGroup;
	
		[Header("Target Box")]

		[SerializeField]
		private Image targetBoxImage;
		private HUDImageObject targetBoxImageObject;
		Vector2 originalTargetBoxSize;
		Vector2 targetBoxSize;

		[SerializeField]
		public float minTargetBoxSize;
		public float targetBoxBuffer;
	
		[Header("Lead Target Box")]

		[SerializeField]
		private Image leadTargetBoxImage;
		private HUDImageObject leadTargetBoxImageObject;
		[SerializeField]
		private float minLeadTargetOffset;
	
		[Header("Lead Target Line")]

		[SerializeField]
		private Image leadTargetLineImage;
		private HUDImageObject leadTargetLineImageObject;
	
		[Header("Offscreen Arrow")]

		[SerializeField]
		private Image arrowImage;
		private HUDImageObject arrowImageObject;

		[Header("Offscreen Arrow - Selected")]

		[SerializeField]
		private Image arrowSelectedImage;
		private HUDImageObject arrowSelectedImageObject;

		[Header("Label")]

		[SerializeField]
		private Text labelText;
		private HUDTextObject labelTextObject;

		[SerializeField]
		private Vector2 labelFieldOffset;
	
		[Header("Distance")]

		[SerializeField]
		private Text distanceText;
		private HUDTextObject distanceTextObject;
		public float displayKMThreshold;

		[SerializeField]
		private Vector2 onScreenValueFieldOffset;

		[Header("Health Bar")]

		float previousBarVal = 0;

		[SerializeField]
		private Image healthBarImage;
		private HUDImageObject healthBarImageObject;

		[SerializeField]
		private Image healthBarBackgroundImage;
		private HUDImageObject healthBarBackgroundImageObject;

		[SerializeField]
		private Vector2 barFieldOffset;
	
		[Header("Lock")]

		[SerializeField]
		private Image lockImage;
		private HUDImageObject lockImageObject;
	
		Coroutine lockAnimCoroutine;

		enum AnimationState{
			Off,
			Running,
			Finished
		}
		AnimationState animationState;
	
		[SerializeField]
		private float lockAnimSpeed = 0.2f;
		[SerializeField]
		public float lockingOffset = 15f;
		[SerializeField]
		public float lockedOffset = 5f;

		
	
		void Awake()
		{
	
			// Prepare all of the image and text objects for easy access

			thisRectTransform = GetComponent<RectTransform>();
			thisGameObject = gameObject;
	
			targetBoxImageObject = new HUDImageObject(targetBoxImage);
			originalTargetBoxSize = targetBoxImageObject.cachedRT.sizeDelta;

			leadTargetBoxImageObject = new HUDImageObject(leadTargetBoxImage);

			leadTargetLineImageObject = new HUDImageObject(leadTargetLineImage);
			
			arrowImageObject = new HUDImageObject(arrowImage);

			arrowSelectedImageObject = new HUDImageObject(arrowSelectedImage);

			lockImageObject = new HUDImageObject(lockImage);
	
			healthBarImageObject = new HUDImageObject(healthBarImage);
		
			healthBarBackgroundImageObject = new HUDImageObject(healthBarBackgroundImage);

			distanceTextObject = new HUDTextObject(distanceText);
	
			labelTextObject = new HUDTextObject(labelText);


			// Used to dynamically adjust alpha of multiple elements
			canvasGroup = GetComponent<CanvasGroup>();

		}
	
		// Anything that needs to be done when enabling the widget
		public void Enable()
		{
			thisGameObject.SetActive(true);
		}

		// Anything that needs to be done when disabling the widget
		public void Disable()
		{
			StopAllCoroutines();
			thisGameObject.SetActive(false);
		}

		public void Set(Visor_WidgetParameters _parameters)
		{
			
			// Set position and rotation of entire widget
			if (_parameters.isWorldSpace)
			{
				thisRectTransform.position = _parameters.targetUIPosition;
			}
			else
			{
				thisRectTransform.anchoredPosition = _parameters.targetUIPosition;
			}
			thisRectTransform.rotation = _parameters.targetUIRotation;

			// Set the scale of the entire widget
			thisRectTransform.localScale = new Vector3(_parameters.scale, _parameters.scale, _parameters.scale);
			
			bool isShowingLock = false;

			// Do settings depending on on or off screen
			if (_parameters.isOnScreen)
			{

				// Enable on-screen elements
				targetBoxImageObject.cachedGO.SetActive(true);

				// Calculate the target box size
				if (_parameters.expandingTargetBoxes)
				{

					// Set the target box size value
					targetBoxSize = _parameters.targetMeshSize / _parameters.scale + new Vector2(targetBoxBuffer * 2, targetBoxBuffer * 2);
					targetBoxSize.x = Mathf.Max(targetBoxSize.x, minTargetBoxSize);
					targetBoxSize.y = Mathf.Max(targetBoxSize.y, minTargetBoxSize);

				}
				else
				{
					targetBoxSize = Vector2.Max(originalTargetBoxSize, new Vector2(minTargetBoxSize, minTargetBoxSize));
				}
					
				// Set the target box size
				targetBoxImageObject.cachedRT.sizeDelta = targetBoxSize;

				// Adjust the position of the label field relative to the target box
				float direction = Mathf.Sign(labelFieldOffset.y);
				labelTextObject.cachedRT.anchoredPosition = labelFieldOffset + new Vector2(0f, targetBoxSize.y / 2 * direction);

				// Adjust the position of the value field relative to the target box
				direction = Mathf.Sign(onScreenValueFieldOffset.y);
				distanceTextObject.cachedRT.anchoredPosition = onScreenValueFieldOffset + new Vector2(0f, targetBoxSize.y / 2 * direction);
				
				// Adjust the position of the bar field relative to the target box
				direction = Mathf.Sign(barFieldOffset.y);
				healthBarImageObject.cachedRT.anchoredPosition = barFieldOffset + new Vector2(0f, targetBoxSize.y / 2 * direction);
				healthBarBackgroundImageObject.cachedRT.anchoredPosition = healthBarImageObject.cachedRT.anchoredPosition;

				// Disable off-screen elements
				arrowImageObject.cachedGO.SetActive(false);
				arrowSelectedImageObject.cachedGO.SetActive(false);

				// If selected target, enable selected target elements
				if (_parameters.isSelectedTarget)
				{

					// Enable locking UI
					lockImageObject.cachedGO.SetActive(true);
					isShowingLock = true;

					// Only show lead target reticle if it is far enough away from center
					if (_parameters.showLeadUI)
					{
		
						// Activate the lead target box/line
						leadTargetBoxImageObject.cachedGO.SetActive(true);
						leadTargetLineImageObject.cachedGO.SetActive(true);

						// Calculate the correct position for the lead target line
						Vector3 _pos = (_parameters.leadTargetUIPosition + _parameters.targetUIPosition) / 2f;
						
						// Set the position/rotation of the lead target box	and line
						if (_parameters.isWorldSpace)
						{

							// Get the lead target separation in world space, undoing all of the scale to ensure correct values
							float leadSeparation = Vector3.Magnitude(_parameters.leadTargetUIPosition - _parameters.targetUIPosition) / _parameters.scale;
							Vector2 size = leadTargetLineImageObject.cachedRT.sizeDelta;
							size.x = leadSeparation;
							leadTargetLineImageObject.cachedRT.sizeDelta = size;

							leadTargetBoxImageObject.cachedRT.position = _parameters.leadTargetUIPosition;
							leadTargetLineImageObject.cachedRT.position = (_parameters.leadTargetUIPosition + _parameters.targetUIPosition) / 2f;

							Vector3 lineForwardVector = (_pos - _parameters.cameraPosition).normalized;
							Vector3 lineUpVector = Quaternion.AngleAxis(90f, lineForwardVector) * (_parameters.leadTargetUIPosition - _parameters.targetUIPosition).normalized;
							leadTargetLineImageObject.cachedRT.LookAt(leadTargetLineImageObject.cachedRT.position + lineForwardVector, lineUpVector);
							
						}
						else
						{

							// Get the lead target separation in world space, undoing all of the scale to ensure correct values
							float leadSeparation = Vector3.Magnitude(_parameters.leadTargetUIPosition / _parameters.scale);
							Vector2 size = leadTargetLineImageObject.cachedRT.sizeDelta;
							size.x = leadSeparation;
							leadTargetLineImageObject.cachedRT.sizeDelta = size;

							leadTargetBoxImageObject.cachedRT.anchoredPosition = _parameters.leadTargetUIPosition;
							leadTargetLineImageObject.cachedRT.anchoredPosition = _parameters.leadTargetUIPosition/2f;

							Vector3 lineForwardVector = Vector3.forward;
							Vector3 lineUpVector = Quaternion.AngleAxis(90f, lineForwardVector) * _parameters.leadTargetUIPosition.normalized;
							leadTargetLineImageObject.cachedRT.LookAt(leadTargetLineImageObject.cachedRT.position + lineForwardVector, lineUpVector);
						
						}
						
						

					}
					else
					{
						// Activate the lead target box/line
						leadTargetBoxImageObject.cachedGO.SetActive(false);
						leadTargetLineImageObject.cachedGO.SetActive(false);
					}
				}
				// Not selected target, so disable selected target UI
				else
				{
					lockImageObject.cachedGO.SetActive(false);
					leadTargetBoxImageObject.cachedGO.SetActive(false);
					leadTargetLineImageObject.cachedGO.SetActive(false);
				}
			}

			// Offscreen
			else
			{
				// Disable on-screen elements
				targetBoxImageObject.cachedGO.SetActive(false);
				leadTargetBoxImageObject.cachedGO.SetActive(false);
				leadTargetLineImageObject.cachedGO.SetActive(false);
				lockImageObject.cachedGO.SetActive(false);

				// enable off-screen elements
				arrowImageObject.cachedGO.SetActive(true);

				// Set arrow rotation
				arrowImageObject.cachedRT.localRotation = Quaternion.Euler(0f, 0f, _parameters.arrowAngle);

				//if (_parameters.isWorldSpace)
				//{
				//Vector3 toArrowVector = arrowImageObject.cachedRT.position - _parameters.cameraPosition;
				//float toArrowVectorAngle = Vector3.Angle(toArrowVector, _parameters.cameraTransform.forward);
				//float tmp = toArrowVector.magnitude / Mathf.Cos(toArrowVectorAngle * Mathf.Deg2Rad);
				//Vector3 arrow_Right = arrowImageObject.cachedRT.position - (_parameters.cameraPosition + _parameters.cameraTransform.forward * tmp);
					
				// Rotate the right vector 90 degrees to get the up vector
				//Vector3 arrow_Up = Quaternion.AngleAxis(90f, toArrowVector) * arrow_Right;

				//arrowImageObject.cachedRT.LookAt(arrowImageObject.cachedRT.position + toArrowVector, arrow_Up);
				//}
				//else
				//{
				//	arrowImageObject.cachedRT.localRotation = Quaternion.Euler(0f, 0f, _parameters.arrowAngle);

				//}

				
				// if selected target, enable selected target off-screen element
				if (_parameters.isSelectedTarget)
				{
					arrowSelectedImageObject.cachedGO.SetActive(true);
				}
				else
				{
					arrowSelectedImageObject.cachedGO.SetActive(false);
				}
			}
			
			// Locking
			if (isShowingLock)
			{
				// Update locking animation
				switch (_parameters.lockState)
				{
					case RadarFunctions.LockState.NoLock:

						// Stop animation
						if (animationState != AnimationState.Off)
						{ 
							StopCoroutine(lockAnimCoroutine);
							animationState = AnimationState.Off;
						}

						// Disable UI
						lockImageObject.cachedGO.SetActive(false);

						break;

					case RadarFunctions.LockState.Locking:
			
						// Stop animation
						if (animationState != AnimationState.Off)
						{ 
							StopCoroutine(lockAnimCoroutine);
							animationState = AnimationState.Off;
						}

						// Set the lock UI size to the locking size and make sure it is activated
						lockImageObject.cachedRT.sizeDelta = targetBoxSize + new Vector2(lockingOffset * 2f, lockingOffset * 2f);
						lockImageObject.cachedGO.SetActive(true);

						break;

					case RadarFunctions.LockState.Locked:

						// If animation isn't running, start it
						if (animationState == AnimationState.Off)
						{
							lockAnimCoroutine = StartCoroutine(LockAnimation());
						}
						else if (animationState == AnimationState.Finished)
						{
							lockImageObject.cachedRT.sizeDelta = targetBoxSize + new Vector2(lockedOffset * 2f, lockedOffset * 2f);
						}

						break;
				}
			}

			
			// Set label field values
			labelText.text = _parameters.labelFieldValue;
			labelTextObject.cachedGO.SetActive(_parameters.showLabelField && _parameters.isOnScreen);
			
			// Set value field values
			distanceText.text = _parameters.valueFieldValue;
			distanceTextObject.cachedGO.SetActive(_parameters.showValueField && _parameters.isOnScreen);
			
			// Only update fill amount when necessary as it causes some GC overhead
			if (Mathf.Abs(_parameters.barFieldValue - previousBarVal) > 0.0001f)
			{
				healthBarImage.fillAmount = _parameters.barFieldValue;
				previousBarVal = _parameters.barFieldValue;
			}
			
			// Update bar field
			healthBarImageObject.cachedGO.SetActive(_parameters.showBarField && _parameters.isOnScreen);
			healthBarBackgroundImageObject.cachedGO.SetActive(_parameters.showBarField && _parameters.isOnScreen);
			
			
			// Color
			targetBoxImage.color = _parameters.widgetColor;
			arrowImage.color = _parameters.widgetColor;
			arrowSelectedImage.color = _parameters.widgetColor;
			lockImage.color = _parameters.widgetColor;
			leadTargetBoxImage.color = _parameters.widgetColor;
			leadTargetLineImage.color = _parameters.widgetColor;
			healthBarImage.color = _parameters.widgetColor;
			labelText.color = _parameters.widgetColor;
			distanceText.color = _parameters.widgetColor;

			canvasGroup.alpha = _parameters.alpha;

		}

		// Coroutine for lock animation
		IEnumerator LockAnimation()
		{

			// Reset
			lockImageObject.cachedRT.sizeDelta = targetBoxSize + new Vector2(lockingOffset * 2f, lockingOffset * 2f);
			float startTime = Time.time;
	
			animationState = AnimationState.Running;
	
			// Animate the locking box
			while (Time.time - startTime < lockAnimSpeed)
			{
				// Shrink over time
				float fraction = (Time.time - startTime)/lockAnimSpeed;
				float currentOffset = fraction * lockedOffset + (1-fraction) * lockingOffset;
				lockImageObject.cachedRT.sizeDelta = targetBoxSize + new Vector2(currentOffset * 2f, currentOffset * 2f);
				yield return null;
			}
			animationState = AnimationState.Finished;
		}
	}
}