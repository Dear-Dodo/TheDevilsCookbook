using TDC.Spellcasting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Utility;

namespace TDC
{
    public class LoadoutSpell : MonoBehaviour, IPointerDownHandler, IPointerUpHandler,IPointerEnterHandler,IPointerExitHandler
    {
        public void OnPointerDown(PointerEventData eventData)
        {
            if (MouseOver)
            {
                IsBeingDragged = true;
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            IsBeingDragged = false;
            if (InSlot)
            {
                if (NewSlot != null)
                {
                    if (NewSlot.Spell != null)
                    {
                        if (CurrentSlot != ParentSlot)
                        {
                            CurrentSlot.Spell = NewSlot.Spell;
                            NewSlot.Spell.CurrentSlot = CurrentSlot;
                        }
                        else if (NewSlot.Spell.ParentSlot != null)
                        {
                            NewSlot.Spell.ParentSlot.Spell = NewSlot.Spell;
                            NewSlot.Spell.CurrentSlot = NewSlot.Spell.ParentSlot;
                        }
                        else
                        {
                            NewSlot = CurrentSlot;
                        }
                    }
                    NewSlot.Spell.OverridePosition = false;
                    if (CurrentSlot.Parent && CurrentSlot != NewSlot)
                    {
                        CurrentSlot.Spell = null;
                    }
                    if (NewSlot.Parent)
                    {
                        if (ParentSlot != null)
                        {
                            NewSlot.Spell = null;
                            CurrentSlot = ParentSlot;
                        }
                        else
                        {
                            return;
                        }
                    }
                    else
                    {
                        CurrentSlot = NewSlot;
                    }
                    CurrentSlot.Spell = this;
                    NewSlot = null;
                }
            }
        }

        void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
        {
            MouseOver = true;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            MouseOver = false;
        }

        public Spell Spell;
        public Image Image;
        public LoadoutManagerUI LoadoutManager; 
        public bool IsBeingDragged;
        public bool OverridePosition;
        public bool MouseOver;
        public bool InSlot;
        public LoadoutSpellSlotUI ParentSlot;
        public LoadoutSpellSlotUI CurrentSlot;
        public LoadoutSpellSlotUI NewSlot;
        public Canvas Canvas;

        // Start is called before the first frame update
        void Start()
        {
        
        }

        // Update is called once per frame
        void Update()
        {
            if (!OverridePosition) {
                if (IsBeingDragged)
                {
                    Canvas.sortingOrder = 2;
                    float dist = float.MaxValue;
                    LoadoutSpellSlotUI closestSlot = null;
                    foreach (LoadoutSpellSlotUI slot in LoadoutManager.Slots)
                    {
                        float currentdist = Vector3.Distance(Mouse.current.position.ReadValue().xyo(), slot.transform.position);
                        if (currentdist < dist)
                        {
                            if ((!(slot.Parent && CurrentSlot == ParentSlot) || slot == ParentSlot) && !(slot.Parent && ParentSlot == null))
                            {
                                dist = currentdist;
                                closestSlot = slot;
                            }
                        }
                    }
                    if (dist < LoadoutManager.SnappingRange && closestSlot.Spell != null)
                    {
                        if (closestSlot != NewSlot)
                        {
                            if (NewSlot != null && NewSlot.Spell != null)
                            {
                                NewSlot.Spell.OverridePosition = false;
                                NewSlot.Spell.Image.color = Color.white;
                            }
                        }
                        if (closestSlot.Parent)
                        {
                            transform.position = ParentSlot.transform.position;
                        }
                        else
                        {
                            transform.position = closestSlot.transform.position;
                        }
                        NewSlot = closestSlot;
                        InSlot = true;
                    } else
                    {
                        transform.position = Mouse.current.position.ReadValue().xyo();
                        if (NewSlot != null && NewSlot.Spell != null)
                        {
                            NewSlot.Spell.OverridePosition = false;
                            NewSlot.Spell.Image.color = Color.white;
                        }
                        NewSlot = null;
                        InSlot = false;

                    }
                    if (InSlot)
                    {
                        Image.color = new Color(0.8f, 0.5f, 0.8f);
                        if (NewSlot != null && NewSlot.Spell != this)
                        {
                            NewSlot.Spell.OverridePosition = true;
                            if (CurrentSlot != ParentSlot)
                            {
                                NewSlot.Spell.transform.position = CurrentSlot.transform.position;
                            } else
                            {
                                if (NewSlot.Spell.ParentSlot != null)
                                {
                                    NewSlot.Spell.transform.position = NewSlot.Spell.ParentSlot.transform.position;
                                } else
                                {
                                    NewSlot.Spell.OverridePosition = false;
                                }
                            }
                            NewSlot.Spell.Image.color = new Color(0.8f,0.5f,0.8f);
                        }
                    }
                    else
                    {
                        Image.color = Color.white;
                    }
                } else
                {
                    Canvas.sortingOrder = 1;
                    Image.color = Color.white;
                    transform.position = CurrentSlot.transform.position;
                }
            }
        }
    }
}
