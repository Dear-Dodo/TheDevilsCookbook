/*
 *  Author: James Greensill
 */

using FMOD.Studio;
using FMODUnity;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Nito.AsyncEx;
using TDC.Core.Manager;
using TDC.Core.Utility;
using TDC.Interactions;
using TDC.Items;
using TDC.Ordering;
using TDC.Player;
using TDC.UI;
using TDC.UI.PatronIndicator;
using UnityAsync;
using UnityEngine;
using UnityEngine.VFX;

namespace TDC.Patrons
{
    [RequireComponent(typeof(Animator), typeof(Inventory))]
    public class Patron : MonoBehaviour, IInteractable
    {
        public GameObject GameObject => gameObject;

        [Header("Required in inspector.")]
        public GameObject RootBone;

        public VisualEffect BurrowEffect;
        public VisualEffect AttackEffect;

        public AnimationClip SpawnClip;
        public AnimationClip LeaveClip;
        public AnimationClip AttackClip;
        public AnimationClip BurrowClip;

        public EventReference AppearSFX;
        public EventReference HissSFX;
        public EventReference BurrowSFX;
        public EventReference BiteSFX;
        public EventReference SatisfiedSFX;
        public EventReference SlideSFX;

        private Order _Order;

        public Order Order
        {
            get => _Order;
            set
            {
                _Order = value;
                OrderAssigned?.Invoke();
            }
        }

        public event Action OrderAssigned;
        public event Action ExitedMap;
        public AsyncManualResetEvent ExitedMapAsync = new AsyncManualResetEvent();

        public Action FinishedEntrance;
        public AsyncManualResetEvent FinishedEntranceAsync = new AsyncManualResetEvent();
        
        internal Vector3 WindowPosition;

        [SerializeField, SerializedValueRequired]
        private ButtonPrompt _ButtonPrompt;

        [SerializeField, SerializedValueRequired]
        private PatronOrderIndicator _OrderIndicator;

        private Collider _Collider;

        private EventInstance _SlideSFX;

        /// <summary>
        /// Modifying this value immediately reflects in the Inspector.
        /// </summary>
        public float Patience
        {
            get => _Patience;
            set
            {
                _Patience = value;
                _Animator.SetFloat(_PatienceHash, _Patience);
            }
        }

        public bool Ready
        {
            get => _Ready;
            set
            {
                _Ready = value;
                _Animator.SetBool(_ReadyHash, _Ready);
            }
        }

        public bool Leave
        {
            get => _Leave;
            set
            {
                _Leave = value;
                _Animator.SetBool(_LeaveHash, _Leave);
            }
        }

        public bool Burrow
        {
            get => _Burrow;
            set
            {
                _Burrow = value;
                _Animator.SetBool(_BurrowHash, _Burrow);
            }
        }

        public bool Attack
        {
            get => _Attack;
            set
            {
                _Attack = value;
                _Animator.SetBool(_AttackHash, _Attack);
            }
        }

        public Inventory Inventory;

        private bool _Ready;
        private bool _Attack;
        private bool _Burrow;
        private bool _Leave;
        private float _Patience;
        private Section _TempCachedSection;
        private Animator _Animator;

        /// <summary>
        /// Constant string for the Leave boolean in the Animator.
        /// </summary>
        private const string KLeaveBool = "anim_Leave";

        /// <summary>
        /// Constant string for the attack boolean in the Animator.
        /// </summary>
        private const string KAttackBool = "anim_Attack";

        /// <summary>
        /// Constant string for the attack boolean in the Animator.
        /// </summary>
        private const string KBurrowBool = "anim_Burrow";

        /// <summary>
        /// Constant string for the ready boolean in the Animator.
        /// </summary>
        private const string KReadyBool = "anim_Ready";

        /// <summary>
        /// Constant string for the Patience float in the Animator.
        /// </summary>
        private const string KPatienceFloat = "anim_Patience";

        private int _LeaveHash;
        private int _AttackHash;
        private int _ReadyHash;
        private int _PatienceHash;
        private int _BurrowHash;

        public void FinishEntrance()
        {
            FinishedEntrance?.Invoke();
            FinishedEntranceAsync.Set();
        }
        
        public void Awake()
        {
            this.Validate();
            Inventory = GetComponent<Inventory>();
            _Animator = GetComponent<Animator>();
            _Collider = GetComponent<Collider>();
            _Collider.enabled = false;
            OrderAssigned += OnOrderAssigned;
            CacheAnimatorHashes();
            _OrderIndicator.gameObject.SetActive(false);
            _ButtonPrompt.gameObject.SetActive(false);
        }

        public void Start()
        {
            _TempCachedSection = Inventory.AddSection(new Section(ItemTypes.OrderableFood, 1, 1));
            _ButtonPrompt.SetButton(GameManager.PlayerControls.Player.Interact);
            _ButtonPrompt.SetPrompt("Serve Food");
            _ButtonPrompt.ClickButton.onClick.AddListener(GameManager.PlayerCharacter.Interact);
            Inventory = GetComponent<Inventory>();
        }

        private void OnDestroy()
        {
            _SlideSFX.release();
            ExitedMap?.Invoke();
            ExitedMapAsync.Set();
        }

        private void CacheAnimatorHashes()
        {
            _LeaveHash = Animator.StringToHash(KLeaveBool);
            _AttackHash = Animator.StringToHash(KAttackBool);
            _ReadyHash = Animator.StringToHash(KReadyBool);
            _BurrowHash = Animator.StringToHash(KBurrowBool);
            _PatienceHash = Animator.StringToHash(KPatienceFloat);
        }

        private void OnOrderAssigned()
        {
            _OrderIndicator.Initialise(_Order);
            _OrderIndicator.gameObject.SetActive(true);
            _Collider.enabled = true;
            SFXHelper.PlayOneshot(AppearSFX, gameObject);
            _SlideSFX = RuntimeManager.CreateInstance(SlideSFX);
            _SlideSFX.set3DAttributes(gameObject.To3DAttributes());
            _SlideSFX.start();
        }

        /// <summary>
        ///  Code retaining to the Patron class.
        /// </summary>
        /// <returns></returns>

        #region Patron

        public async Task PatronAttack()
        {
            _OrderIndicator.gameObject.SetActive(false);

            SFXHelper.PlayOneshot(BurrowSFX, gameObject);

            VisualEffect burrowVfx = Instantiate(BurrowEffect, transform.position + -transform.right * 2.15f + transform.forward * .25f, Quaternion.identity);
            burrowVfx.SetFloat("Lifetime", BurrowClip.length + 1);
            _Collider.enabled = false;
            Burrow = true;
            await Await.Seconds(BurrowClip.length);
            Burrow = false;

            Destroy(burrowVfx.gameObject, 1.0f);

            VisualEffect attackVfx = Instantiate(AttackEffect, GameManager.PlayerCharacter.transform.position, Quaternion.identity, GameManager.PlayerCharacter.transform);
            var attackTime = attackVfx.GetFloat("Lifetime");

            await Await.Seconds(attackTime / 1.25f);

            Attack = true;
            transform.position = GameManager.PlayerCharacter.transform.position + new Vector3(0, -2f, 0);
            SFXHelper.PlayOneshot(BiteSFX, gameObject);
            PlayerStats stats = await GameManager.PlayerCharacter.GetPlayerStats();
            stats.Damage(1);
            await Await.Seconds(AttackClip.length);
            Attack = false;

            await Await.Seconds(2.5f);

            Destroy(attackVfx.gameObject);
            Destroy(gameObject);
        }

        public async Task PatronLeave()
        {
            SFXHelper.PlayOneshot(HissSFX, gameObject);
            _Collider.enabled = false;
            _OrderIndicator.gameObject.SetActive(false);
            _Animator.SetBool(_LeaveHash, true);
            await Await.Seconds(LeaveClip.length);
        }

        #endregion Patron

        /// <summary>
        ///  Code retaining to the IInteractable interface.
        /// </summary>
        /// <returns></returns>

        #region IInteractable

        public void OnHover(Interactor interactor)
        {
            _ButtonPrompt.gameObject.SetActive(true);
        }

        public void ExitHover(Interactor interactor)
        {
            _ButtonPrompt.gameObject.SetActive(false);
        }

        public void Interact(Interactor interactor, Interaction interaction)
        {
            switch (interaction)
            {
                case Interaction.Deposit:
                    Dictionary<StorableObject, int> consumedItems = new Dictionary<StorableObject, int>();
                    if (interactor.Inventory.TryWithdrawItems(out consumedItems, Query.MakeQuery(Order.Food.Output)))
                    {
                        GameManager.OrderManager.CompleteOrder(Order, true);
                    }
                    break;

                case Interaction.Inspect:
                    /*void*/
                    break;

                default:
                    break;
            }
        }

        public Interaction GetInteractions(Interactor interactor)
        {
            if (Attack || Burrow)
            {
                return Interaction.None;
            }
            else
            {
                return Interaction.Deposit;
            }
        }

        #endregion IInteractable
    }
}