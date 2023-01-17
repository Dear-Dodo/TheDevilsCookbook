using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using TDC.Cooking;
using TDC.Core.Manager;
using TDC.Ingredient;
using TDC.Items;
using TDC.Ordering;
using TDC.Player;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

using Button = UnityEngine.UI.Button;

namespace TDC.Debugging
{
    public class DebugConsole : MonoBehaviour
    {
        delegate bool RunCommand(out string error, params string[] args);
        Dictionary<string, RunCommand> Commands;
        public GameObject ConsolePanel;
        public RectTransform Logs;
        public ScrollRect Scroll;
        public DebugLog Template;
        public TMP_InputField InputField;
        public TextMeshProUGUI StackTraceText;
        public Button CloseButton;
        public WindowResizeRegion[] WindowResizeRegions;
        public bool CheatsEnabled = false;
        public bool Godmode;
        public bool Noclip;
        private bool _GameRunning = false;
        private List<string> _History = new List<string>();
        private int _HistoryIndex = 0;
        private string _AutoComplete = "";
        private float _Timescale = 1;
        private List<DebugLog> _LogCounts = new List<DebugLog>();

        private bool _InputFocusLastState = false;
        
        private enum AutoFillMethods
        {
            NONE,
            COMMAND,
            STRING,
            INT,
            BOOL,
            RECIPE,
            INGREDIENT,
            ITEM,
            SCENE
        }
        private delegate string AutofillMethod(string currentText,ParameterAttribute parameterAttribute);
        private Dictionary<AutoFillMethods, AutofillMethod> AutofillDict;


        private void Awake()
        {
            GameManager.RunOnInitialisation(Initialise);
        }

        void Initialise()
        {
            DontDestroyOnLoad(gameObject);
            ConsolePanel.SetActive(false);
            Commands = new Dictionary<string, RunCommand>() {
                {"help",new RunCommand(Help)},
                {"pact",new RunCommand(EnableCheats)},
                {"kill",new RunCommand(Kill)},
                {"endlevel",new RunCommand(EndLevel)},
                {"damage",new RunCommand(Damage)},
                {"godmode",new RunCommand(GodMode)},
                {"noclip",new RunCommand(NoClip)},
                {"order",new RunCommand(Order)},
                {"spawn",new RunCommand(Spawn)},
                {"give",new RunCommand(Give)},
                {"money",new RunCommand(Money)},
                {"timescale",new RunCommand(Timescale)},
                {"load",new RunCommand(Load)},
                {"reload",new RunCommand(Reload)}
            };
            AutofillDict = new Dictionary<AutoFillMethods, AutofillMethod>(){
            {AutoFillMethods.NONE, new AutofillMethod(AutofillNone)},
            {AutoFillMethods.COMMAND, new AutofillMethod(AutofillCommand)},
            {AutoFillMethods.STRING, new AutofillMethod(AutofillString)},
            {AutoFillMethods.INT, new AutofillMethod(AutofillInt)},
            {AutoFillMethods.BOOL, new AutofillMethod(AutofillBool)},
            {AutoFillMethods.RECIPE, new AutofillMethod(AutofillRecipe)},
            {AutoFillMethods.INGREDIENT, new AutofillMethod(AutofillIngredient)},
            {AutoFillMethods.ITEM, new AutofillMethod(AutofillItem)},
            {AutoFillMethods.SCENE, new AutofillMethod(AutofillScene)},
            };
            GameManager.PlayerControls.UI.Console.performed += (context) => { ConsolePanel.SetActive(!ConsolePanel.activeSelf);  if (ConsolePanel.activeSelf) InputField.ActivateInputField(); };
            CloseButton.onClick.AddListener(() => { ConsolePanel.SetActive(false); });
            GameManager.PlayerControls.UI.Submit.started += (context) => ProcessString(InputField.text);
            GameManager.PlayerControls.UI.Up.started += (context) => OnUpKeyPressed();
            GameManager.PlayerControls.UI.Down.started += (context) => OnDownKeyPressed();
            GameManager.PlayerControls.UI.Complete.started += (context) => Autofill();
            Application.logMessageReceived += LogMessageReceived;
            Application.quitting += () => { _GameRunning = false; };
            foreach (WindowResizeRegion windowResizeRegion in WindowResizeRegions)
            {
                windowResizeRegion.OnResize += ScrollToBottom;
            }
            _GameRunning = true;
        }

        private void OnInputFocused()
        {
            GameManager.PlayerControls.Player.Disable();
            GameManager.Timescale = 0;
        }

        private void OnInputFocusLost()
        {
            GameManager.PlayerControls.Player.Enable();
            GameManager.Timescale = _Timescale;
        }

        private void CheckInputFocusEvents()
        {
            if (!_InputFocusLastState && InputField.isFocused) OnInputFocused();
            else if (_InputFocusLastState && !InputField.isFocused) OnInputFocusLost();
            _InputFocusLastState = InputField.isFocused;
        }
        
        // Update is called once per frame
        void Update()
        {
            StackTraceText.text = "";
            CheckInputFocusEvents();

            if (Godmode && GameManager.PlayerCharacter.GetPlayerStats().Result.Health.Value > 0)
            {
                GameManager.PlayerCharacter.GetPlayerStats().Result.Health.Value = GameManager.PlayerCharacter.GetPlayerStats().Result.MaxHealth;
            }

            if (Noclip)
            {
                GameManager.PlayerCharacter.GetComponent<Collider>().enabled = false;
            }
        }

        private void LateUpdate()
        {
            string currentText;
            if (InputField.text.Contains("<color=#545454>"))
            {
                currentText = InputField.text.Substring(0, Mathf.Max(InputField.stringPosition, InputField.text.IndexOf("<color=#545454>")));
            } else
            {
                currentText = InputField.text.Substring(0, InputField.stringPosition);
            }
            List<string> autoCompleteOptions = new List<string>();
            if (Commands.Keys.Contains(currentText.Split(' ')[0]))
            {
                string[] currentArguments = currentText.Split(' ');
                ParameterAttribute[] att = (ParameterAttribute[])Attribute.GetCustomAttributes(Commands[currentArguments[0]].Method, typeof(ParameterAttribute));
                int parameterGroup = -1;
                if (currentArguments.Length > 2)
                {
                    foreach (ParameterAttribute param in att)
                    {
                        if (currentArguments[currentArguments.Length - 2] == param.ParameterString)
                        {
                            parameterGroup = param.ParameterGroupIndex;
                        }
                    }
                }
                if (att.Length > 0)
                {
                    foreach (ParameterAttribute param in att)
                    {
                        if (currentArguments.Length - 2 == param.ParameterIndex && (parameterGroup == param.ParameterGroupIndex || parameterGroup == -1))
                        {
                            string option = AutofillDict[(AutoFillMethods)param.ParameterAutofillMethod].Invoke(currentText, param);
                            if (option != "")
                            {
                                autoCompleteOptions.Add(option);
                            }
                        }
                    }
                }
                if (autoCompleteOptions.Count > 0)
                {
                    currentText.Substring(0, currentText.LastIndexOf(' '));
                    _AutoComplete = currentText.Substring(0, currentText.LastIndexOf(' ') + 1) + autoCompleteOptions[0];
                } else
                {
                    _AutoComplete = "";
                }
            }
            else
            {
                autoCompleteOptions = Commands.Keys.Where(s => s.StartsWith(currentText)).ToList();
                if (autoCompleteOptions.Count() > 0 && !currentText.Equals(""))
                {
                    _AutoComplete = autoCompleteOptions.First();
                }
                else
                {
                    _AutoComplete = "";
                }
            }
            if (currentText.Length < _AutoComplete.Length && currentText.Length != 0 && (InputField.stringPosition == InputField.text.Length || InputField.text.Contains("<color=#545454>")))
            {
                currentText += "<color=#545454>" + _AutoComplete.Substring(currentText.Length, _AutoComplete.Length - currentText.Length);
                int autoCompleteIndex = InputField.stringPosition;
                InputField.text = currentText;
                InputField.stringPosition = autoCompleteIndex;
                InputField.ForceLabelUpdate();
            } else
            {
                if (InputField.text.Contains("<color=#545454>"))
                {
                    InputField.text = InputField.text.Substring(0, InputField.text.IndexOf("<color=#545454>"));
                }
            }
        }

        public void ScrollToBottom()
        {
            Scroll.normalizedPosition = new Vector2(0, 0);
        }

        private async void LogMessageReceived(string condition, string stackTrace, LogType type)
        {
            if (_GameRunning)
            {
                foreach (DebugLog debugLog in _LogCounts)
                {
                    if (debugLog.log.StackTrace.Equals(stackTrace))
                    {
                        debugLog.Count++;
                        return;
                    }
                }
                DebugLog Log = Instantiate(Template, Logs);
                Log.log = new Log(condition, stackTrace, type);
                Log.StackTraceText = StackTraceText;
                _LogCounts.Add(Log);
                await Task.Delay(100);
                ScrollToBottom();
            }
        }

        async void OnUpKeyPressed()
        {
            _HistoryIndex--;
            if (_HistoryIndex < 0) _HistoryIndex = 0;
            InputField.text = _History[_HistoryIndex];
            await Task.Delay(10);
            InputField.stringPosition = InputField.text.Length;
            InputField.ForceLabelUpdate();
        }

        async void OnDownKeyPressed()
        {
            _HistoryIndex++;
            if (_HistoryIndex > _History.Count() - 1) _HistoryIndex = _History.Count() - 1;
            InputField.text = _History[_HistoryIndex];
            await Task.Delay(10);
            InputField.stringPosition = InputField.text.Length;
            InputField.ForceLabelUpdate();
        }

        async void Autofill()
        {
            InputField.text = _AutoComplete;
            await Task.Delay(10);
            InputField.stringPosition = InputField.text.Length;
            InputField.ForceLabelUpdate();
        }

        async void ProcessString(string input)
        {
            string[] inputArray = input.Split(' ');
            string command = inputArray[0];
            string[] args = inputArray.Skip(1).ToArray();
            _History.Add(input);
            _HistoryIndex = _History.Count();
            if (!CheatsEnabled && !(command.ToLower().Equals("pact") || command.ToLower().Equals("help")))
            {
                Debug.LogWarning("Debug Mode Disabled");
                return;
            }
            if (Commands.ContainsKey(command.ToLower()))
            {
                string error;
                if (!Commands[command.ToLower()].Invoke(out error,args))
                {
                    Debug.LogWarning(error);
                }
            } else
            {
                Debug.LogWarning("Invalid Command");
            }
            InputField.text = "";
            InputField.ActivateInputField();
        }

        bool StringToBool(string input, out bool parsed)
        {
            parsed = true;
            if (input != null)
            {
                switch (input.ToLower())
                {
                    case ("0"):
                        return false;
                    case ("1"):
                        return true;
                    case ("false"):
                        return false;
                    case ("true"):
                        return true;
                    case ("disable"):
                        return false;
                    case ("enable"):
                        return true;
                }
            }
            parsed = false;
            return false;
        }

        #region commands

        #region Help
        [Description("Lists all commands")]
        [Parameter(typeof(Nullable),"none",0, (int)AutoFillMethods.NONE)]
        [Parameter(typeof(string),"commandName",0, (int)AutoFillMethods.COMMAND)]
        [Parameter(typeof(string),"key",0, (int)AutoFillMethods.STRING)]
        bool Help(out string error, params string[] args) {
            string output;
            if (args.Length > 0)
            {
                if (Commands.Keys.Contains(args[0]))
                {
                    output = args[0] + ":\n";
                    string description;
                    if (TryGetDescription(Commands[args[0]].Method, out description))
                    {
                        output += description + '\n';
                    }
                    string parameters;
                    if (TryGetParams(Commands[args[0]].Method, out parameters))
                    {
                        output += "Parameters: " + parameters + '\n';
                    }
                    Debug.Log(output);
                    error = "";
                    return true;
                } else if (args[0].Equals("key"))
                {
                    output = "Parameter types: \n";
                    foreach (Type t in ParameterAttribute.symbolKey.Keys)
                    {
                        if (t != typeof(Nullable))
                        {
                            output += char.ToUpper(t.Name[0]) + t.Name.Substring(1) + ": " + ParameterAttribute.symbolKey[t].Replace('*',' ') + ", ";
                        }
                    }
                    output = output.Substring(0, output.Length - 2);
                    Debug.Log(output);
                    error = "";
                    return true;
                } else
                {
                    return InvalidParameter(MethodBase.GetCurrentMethod(), out error); ;
                }
            }
            output = "Commands:\n";
            foreach (string command in Commands.Keys)
            {
                string description;
                if (TryGetDescription(Commands[command].Method,out description)) {
                    output += char.ToUpper(command[0]) + command.Substring(1) + ": " + description + '\n';
                }
            }

            Debug.Log(output);
            error = "";
            return true; 
        }

        #endregion
        #region EnableCheats
        [Description("Enables debug mode")]
        [Parameter(typeof(Nullable),"none",0,(int)AutoFillMethods.NONE)]
        [Parameter(typeof(bool), "enable",0, (int)AutoFillMethods.BOOL)]
        bool EnableCheats(out string error, params string[] args) {
            error = "";
            bool parsed = false;
            if (args.Length != 0) {
                bool result = StringToBool(args[0], out parsed);
                CheatsEnabled = parsed ? result : CheatsEnabled;
                if (!parsed)
                {
                    return InvalidParameter(MethodBase.GetCurrentMethod(), out error); ;
                }
            }
            if (CheatsEnabled)
            {
                Debug.Log("Debug Mode Enabled");
            } else
            {
                Debug.Log("Debug Mode Disabled");
            }
            return parsed;
        }
        #endregion
        #region Kill
        [Description("Kills ingredients, or the player if not specified")]
        [Parameter(typeof(Nullable), "none",0, (int)AutoFillMethods.NONE)]
        [Parameter(typeof(string), "all",0, (int)AutoFillMethods.STRING)]
        [Parameter(typeof(int), "count",0, (int)AutoFillMethods.INT)]
        bool Kill(out string error, params string[] args) {
            error = "";
            if (GameManager.LevelRunning)
            {
                if (args.Length == 0)
                {
                    GameManager.PlayerCharacter.GetPlayerStats().Result.Health.Value = 0;
                    return true;
                }
                else
                {
                    List<Creature> Ingredients = new List<Creature>();
                    Ingredients = FindObjectsOfType<Creature>().ToList();
                    if (args[0].ToLower().Equals("all"))
                    {
                        for (int i = 0; i < Ingredients.Count; i++)
                        {
                            Destroy(Ingredients[i].Catch().gameObject);
                        }
                        return true;
                    }
                    else
                    {
                        int count;
                        if (int.TryParse(args[0], out count))
                        {
                            Creature[] CreaturesSorted = new Creature[count];
                            for (int i = 0; i < count; i++)
                            {

                                float dist = float.MaxValue;
                                foreach (Creature Ingredient in Ingredients)
                                {
                                    float currentDist = (GameManager.PlayerCharacter.transform.position - Ingredient.transform.position).sqrMagnitude;
                                    if (currentDist < dist && !CreaturesSorted.Contains(Ingredient))
                                    {
                                        dist = currentDist;
                                        CreaturesSorted[i] = Ingredient;
                                    }
                                }
                            }
                            for (int i = 0; i < CreaturesSorted.Length; i++)
                            {
                                Destroy(CreaturesSorted[i].Catch().gameObject);
                            }
                            return true;
                        }  else
                        {
                            return InvalidParameter(MethodBase.GetCurrentMethod(), out error); ;
                        }
                    }
                }
            } else
            {
                error = "No Active Level";
            }
            return false;
        }
        #endregion
        #region EndLevel
        [Description("Ends the level")]
        [Parameter(typeof(Nullable), "none", 0, (int)AutoFillMethods.NONE)]
        bool EndLevel(out string error, params string[] args)
        {
            error = "";
            if (GameManager.LevelRunning)
            {
                GameManager.EndLevel();
            }
            else
            {
                error = "No Active Level";
            }
            return false;
        }
        #endregion
        #region Damage
        [Description("Damage the player a specified amount")]
        [Parameter(typeof(int),"damage",0, (int)AutoFillMethods.INT)]
        bool Damage(out string error, params string[] args) {
            if (int.TryParse(args[0],out int damage))
            {
                GameManager.PlayerCharacter.GetPlayerStats().Result.Damage(damage);
                error = "";
                return true;
            } else
            {
                return InvalidParameter(MethodBase.GetCurrentMethod(), out error); ;
            }
        }
        #endregion
        #region GodMode

        private int GodModeModifyDamage(PlayerStats _, int value) => 0;
        
        [Description("Disables taking damage")]
        [Parameter(typeof(Nullable), "none",0, (int)AutoFillMethods.NONE)]
        bool GodMode(out string error, params string[] args) {
            if (GameManager.LevelRunning)
            {
                PlayerStats stats = GameManager.PlayerCharacter.GetPlayerStats().Result;
                if (!Godmode)
                {
                    Debug.Log("Godmode Enabled");
                    stats.PlayerDamageModifiers += GodModeModifyDamage;
                }
                else
                {
                    Debug.Log("Godmode Disabled");
                    stats.PlayerDamageModifiers -= GodModeModifyDamage;
                }
                Godmode = !Godmode;
                error = "";
                return true;
            }
            else
            {
                error = "No Active Level";
            }
            return false;
        }
        #endregion
        #region Noclip
        [Description("Disables collision")]
        [Parameter(typeof(Nullable), "none",0, (int)AutoFillMethods.NONE)]
        bool NoClip(out string error, params string[] args) {

            if (GameManager.LevelRunning)
            {
                if (!Noclip)
                {
                    Debug.Log("Noclip Enabled");
                }
                else
                {
                    Debug.Log("Noclip Disabled");
                    GameManager.PlayerCharacter.GetComponent<Collider>().enabled = true;
                }
                Noclip = !Noclip;
                error = "";
                return true;
            }
            else
            {
                error = "No Active Level";
            }
            return false;
        }
        #endregion
        #region Order
        [Description("Creates and removes orders")]
        [Parameter(typeof(Nullable), "none", 0, 0, (int)AutoFillMethods.NONE)]
        [Parameter(typeof(string), "create", 0, 1, (int)AutoFillMethods.STRING)]
            [Parameter(typeof(Nullable), "none", 1, 1, (int)AutoFillMethods.NONE)]
            [Parameter(typeof(string), "food", 1, 1, (int)AutoFillMethods.RECIPE)]
            [Parameter(typeof(int), "food", 1, 1, (int)AutoFillMethods.INT)]
            [Parameter(typeof(int), "patience", 2, 1, (int)AutoFillMethods.INT)]
        [Parameter(typeof(string), "remove", 0, 2, (int)AutoFillMethods.STRING)]
            [Parameter(typeof(int), "index", 1, 2, (int)AutoFillMethods.INT)]
        bool Order(out string error, params string[] args) {
            error = "";
            if (args.Length == 0)
            {
                string output = "";
                foreach (Order order in GameManager.OrderManager.ActiveOrders)
                {
                    output += GameManager.OrderManager.ActiveOrders.IndexOf(order) + ": " + order.Food.Output[0].Name + ", " + order.ElapsedTime.ToString("0.##") + "/" + order.Time;
                }
                Debug.Log(output);
                return true;
            }
            else
            {
                if (args[0].Equals("create"))
                {
                    if (args.Length == 1)
                    {
                        GameManager.OrderManager.CreateNextOrder();
                        return true;
                    }
                    else if (args.Length == 2)
                    {
                        if (int.TryParse(args[1], out int index))
                        {
                            Recipe recipe = GameManager.CurrentLevelData.RecipePool[index];
                            GameManager.OrderManager.CreateNextOrder(recipe);
                            return true;
                        }
                        else
                        {
                            foreach (Recipe recipe in GameManager.CurrentLevelData.RecipePool)
                            {
                                if (recipe.Output[0].Name.ToLower().Equals(args[1].ToLower()))
                                {
                                    GameManager.OrderManager.CreateNextOrder(recipe);
                                    return true;
                                }
                            }
                        }
                        return InvalidParameter(MethodBase.GetCurrentMethod(), out error);
                    }
                }
                else if (args[0].Equals("remove") && args.Length >= 2 && int.TryParse(args[1], out int index))
                {
                    bool success = true;
                    if (args.Length == 3) success = StringToBool(args[2], out bool parsed);
                    GameManager.OrderManager.CompleteOrder(GameManager.OrderManager.ActiveOrders[index], success);
                    return true;
                }
            }
            return InvalidParameter(MethodBase.GetCurrentMethod(), out error);
        }
        #endregion
        #region Spawn
        [Description("Spawns Ingredients")]
        [Parameter(typeof(Nullable), "none", 0, (int)AutoFillMethods.NONE)]
        [Parameter(typeof(string), "food", 0, (int)AutoFillMethods.INGREDIENT)]
        bool Spawn(out string error, params string[] args) {
            error = "";
            if (GameManager.LevelRunning)
            {
                if (args.Length == 0)
                {
                    string output = "Ingredients: \n";
                    foreach (StorableObject ingredient in GameManager.OrderManager.Ingredients)
                    {
                        output += ingredient.Name + "\n";
                    }
                    Debug.Log(output);
                    return true;
                }else if (args.Length <= 1)
                {
                    StorableObject ingredient = GameManager.OrderManager.Ingredients.FirstOrDefault(i => i.Name.ToLower().Equals(args[0].ToLower()));
                    if (ingredient != null)
                    {
                        int quantity = 1;
                        if (args.Length > 1 && !int.TryParse(args[1], out quantity)) {
                            return InvalidParameter(MethodBase.GetCurrentMethod(), out error);
                        }
                        List<Creature> creatures = new List<Creature>();
                        for (int i = 0; i < quantity; i++)
                        {
                            Creature creature = CreatureManager.CreateCreature(((Food)ingredient).Creature, GameManager.PlayerCharacter.transform.position);
                            creature.Activate();
                            creatures.Add(creature);
                        }
                        FindObjectOfType<SoulImbuer>().OnSpawnFood?.Invoke(creatures);
                        return true;
                    }
                }
                return InvalidParameter(MethodBase.GetCurrentMethod(), out error);
            }
            error = "No Active Level";
            return false;
        }
        #endregion
        #region Give
        [Description("Give Items")]
        [Parameter(typeof(Nullable), "none", 0, (int)AutoFillMethods.NONE)]
        [Parameter(typeof(string), "item", 0, (int)AutoFillMethods.ITEM)]
        bool Give(out string error, params string[] args) {
            error = "";
            if (GameManager.LevelRunning)
            {
                List<StorableObject> Items = new List<StorableObject>(GameManager.OrderManager.Ingredients);
                foreach (Recipe recipe in GameManager.CurrentLevelData.RecipePool)
                {
                    Items.Add(recipe.Output[0]);
                }
                if (args.Length == 0)
                {
                    string output = "Items: \n";
                    foreach (StorableObject item in Items)
                    {
                        output += item.Name + "\n";
                    }
                    Debug.Log(output);
                    return true;
                } else if (args.Length >= 1)
                {
                    StorableObject Item = Items.FirstOrDefault(item => item.Name.ToLower().Equals(args[0].ToLower()));
                    if (Item != null)
                    {
                        int quantity = 1;
                        if (args.Length >= 2)
                        {
                            if (!int.TryParse(args[1], out quantity))
                            {
                                return InvalidParameter(MethodBase.GetCurrentMethod(), out error);
                            }
                        }
                        ItemTypes itemType = GameManager.OrderManager.Ingredients.Contains(Item) ? ItemTypes.Ingedient : ItemTypes.OrderableFood;
                        GameManager.PlayerCharacter.Inventory.DepositItems(new Dictionary<StorableObject, int> { { Item, quantity } }, itemType, out _);
                        return true;
                    }
                }
                return InvalidParameter(MethodBase.GetCurrentMethod(), out error);
            }
            error = "No Active Level";
            return false;
        }
        #endregion
        #region Money
        [Description("Give Money")]
        [Parameter(typeof(Nullable), "none", 0, (int)AutoFillMethods.NONE)]
        [Parameter(typeof(string), "set", 0, (int)AutoFillMethods.STRING)]
        [Parameter(typeof(string), "add", 0, (int)AutoFillMethods.STRING)]
        [Parameter(typeof(int), "amount", 1, (int)AutoFillMethods.INT)]
        bool Money(out string error, params string[] args) {
            error = "";
            if (args.Length >= 2)
            {
                float money;
                if (args[0].Equals("set") && float.TryParse(args[1],out money))
                {
                    GameManager.PlayerCharacter.GetPlayerStats().Result.Currency.Value = (int)(money);
                    return true;
                } else if (args[0].Equals("add") && float.TryParse(args[1], out money))
                {
                    GameManager.PlayerCharacter.GetPlayerStats().Result.Currency.Value += (int)(money);
                    return true;
                }
                return InvalidParameter(MethodBase.GetCurrentMethod(), out error);
            } else
            {
                Debug.Log(GameManager.PlayerCharacter.GetPlayerStats().Result.Currency.Value);
            }
            return true; 
        }
        #endregion
        #region Timescale
        [Description("Set Timescale")]
        [Parameter(typeof(Nullable), "none", 0, (int)AutoFillMethods.NONE)]
        [Parameter(typeof(int), "speed", 0, (int)AutoFillMethods.INT)]
        bool Timescale(out string error, params string[] args) { 
            error = "";
            if (args.Length != 0)
            {
                if (!float.TryParse(args[0],out float timeScale))
                {
                    return InvalidParameter(MethodBase.GetCurrentMethod(), out error);
                }
                Time.timeScale = timeScale;
                _Timescale = timeScale;
                return true;
            }
            Debug.Log("Timescale: " + Time.timeScale);
            return true;
        }
        #endregion
        #region Load
        [Description("Load Scene")]
        [Parameter(typeof(Nullable), "none", 0, (int)AutoFillMethods.NONE)]
        [Parameter(typeof(string), "scene", 0, (int)AutoFillMethods.SCENE)]
        bool Load(out string error, params string[] args) {
            error = "";
            if (args.Length > 0)
            {
                string target = "";
                foreach (string arg in args)
                {
                    target += arg + ' ';
                }
                target = target.Substring(0,target.Length - 1);
                if (GameManager.SceneLoader.TryGetScene(target, out _))
                {
                    if (GameManager.LevelRunning)
                        GameManager.EndLevel();
                    GameManager.SceneLoader.LoadScene(target);
                    return true;
                } else if (GameManager.LevelLoader.Levels.Any(level => level.SceneName.Equals(target)))
                {
                    LevelEntry Level = GameManager.LevelLoader.Levels.First(level => level.SceneName.Equals(target));
                    int number = GameManager.LevelLoader.Levels.IndexOf(Level);
                    if (GameManager.LevelRunning)
                    {
                        GameManager.EndLevel();
                    }
                    LoadAndInitialiseLevel(number);
                    return true;
                }
                return InvalidParameter(MethodBase.GetCurrentMethod(), out error);
            } else
            {
                List<string> options = new List<string>(GameManager.SceneLoader.Scenes.Keys);
                foreach (LevelEntry level in GameManager.LevelLoader.Levels)
                {
                    options.Add(level.SceneName);
                }
                string output = "Scenes: \n";
                foreach (string s in options)
                {
                    output += s + "\n";
                }
                Debug.Log(output);
            }
            return true;
        }
        #endregion
        #region Reload
        [Description("Reload Level")]
        [Parameter(typeof(Nullable), "none", 0, (int)AutoFillMethods.NONE)]
        bool Reload(out string error, params string[] args) {
            error = ""; 
            if (GameManager.LevelRunning)
            {
                GameManager.EndLevel();
                LoadAndInitialiseLevel(GameManager.CurrentLevel);
            } else
            {
                GameManager.SceneLoader.LoadScene("Main Menu");
            }
            return true; 
        }
        #endregion
        #endregion

        #region Autofill
        string AutofillNone(string currentText, ParameterAttribute parameterAttribute)
        {
            return "";
        }

        string AutofillCommand(string currentText, ParameterAttribute parameterAttribute)
        {
            string[] options = Commands.Keys.Where(s => s.ToLower().StartsWith(currentText.ToLower().Split(' ')[parameterAttribute.ParameterIndex + 1])).ToArray();
            if (options.Length == 0)
            {
                return "";
            } else
            {
                return options[0];
            }
        }

        string AutofillString(string currentText, ParameterAttribute parameterAttribute)
        {
            if (parameterAttribute.ParameterString.ToLower().StartsWith(currentText.ToLower().Split(' ')[parameterAttribute.ParameterIndex + 1]))
            {
                return parameterAttribute.ParameterString;
            }
            return "";
        }

        string AutofillInt(string currentText, ParameterAttribute parameterAttribute)
        {
            return "0";
        }

        string AutofillBool(string currentText, ParameterAttribute parameterAttribute)
        {
            List<string> validOptions = new List<string>()
            {
                "true",
                "false",
                "0",
                "1",
                "disable",
                "enable"
            };
            string[] options = validOptions.Where(s => s.ToLower().StartsWith(currentText.ToLower().Split(' ')[parameterAttribute.ParameterIndex + 1])).ToArray();
            if (options.Length == 0)
            {
                return "true";
            }
            else
            {
                return options[0];
            }
        }
        string AutofillRecipe(string currentText, ParameterAttribute parameterAttribute)
        {
            if (GameManager.LevelRunning)
            {
                Recipe[] options = GameManager.CurrentLevelData.RecipePool.Where(s => s.Output[0].Name.ToLower().StartsWith(currentText.ToLower().Split(' ')[parameterAttribute.ParameterIndex + 1])).ToArray();
                if (options.Length == 0)
                {
                    return "";
                }
                else
                {
                    return options[0].Output[0].Name;
                }
            }
            return "";
        }
        string AutofillIngredient(string currentText, ParameterAttribute parameterAttribute)
        {
            if (GameManager.LevelRunning)
            {
                StorableObject[] options = GameManager.OrderManager.Ingredients.Where(s => s.Name.ToLower().StartsWith(currentText.ToLower().Split(' ')[parameterAttribute.ParameterIndex + 1])).ToArray();
                if (options.Length == 0)
                {
                    return "";
                }
                else
                {
                    return options[0].Name;
                }
            }
            return "";
        }

        string AutofillScene(string currentText, ParameterAttribute parameterAttribute)
        {
            List<string> options = GameManager.SceneLoader.Scenes.Keys.Where(s => s.ToLower().StartsWith(currentText.ToLower().Split(' ')[parameterAttribute.ParameterIndex + 1])).ToList();
            foreach (LevelEntry level in GameManager.LevelLoader.Levels.Where(s => s.SceneName.ToLower().StartsWith(currentText.ToLower().Split(' ')[parameterAttribute.ParameterIndex + 1])))
            {
                options.Add(level.SceneName);
            }
            if (options.Count == 0)
            {
                return "";
            }
            else
            {
                return options[0];
            }
            return "";
        }

        string AutofillItem(string currentText, ParameterAttribute parameterAttribute)
        {
            if (GameManager.LevelRunning)
            {
                List<StorableObject> Items = new List<StorableObject> (GameManager.OrderManager.Ingredients);
                foreach (Recipe recipe in GameManager.CurrentLevelData.RecipePool)
                {
                    Items.Add(recipe.Output[0]);
                }
                StorableObject[] options = Items.Where(s => s.Name.ToLower().StartsWith(currentText.ToLower().Split(' ')[parameterAttribute.ParameterIndex + 1])).ToArray();
                if (options.Length == 0)
                {
                    return "";
                }
                else
                {
                    return options[0].Name;
                }
            }
            return "";
        }
        #endregion

        public bool TryGetDescription(MemberInfo Method, out string Description)
        {
            DescriptionAttribute att = (DescriptionAttribute)Attribute.GetCustomAttribute(Method, typeof(DescriptionAttribute));
            if (att != null)
            {
                Description = att.DescriptionString;
                return true;
            }
            else
            {
                Description = "";
                return false;
            }
        }

        public bool TryGetParams(MemberInfo Method, out string Parameters)
        {
            ParameterAttribute[] att = (ParameterAttribute[])Attribute.GetCustomAttributes(Method, typeof(ParameterAttribute));
            if (att.Length != 0)
            {
                Parameters = "";
                foreach (ParameterAttribute param in att)
                {
                    string symbol = ParameterAttribute.symbolKey[param.ParameterType];
                    Parameters += symbol.Substring(0, symbol.Contains('*') ? symbol.IndexOf('*') : symbol.Length) + param.ParameterString +
                        (symbol.Contains('*') ? symbol.Substring(symbol.IndexOf('*') + 1, symbol.Length - symbol.IndexOf('*') - 1) : "") + ", ";
                }
                Parameters = Parameters.Substring(0, Parameters.Length - 2);
                return true;
            }
            else
            {
                Parameters = "";
                return false;
            }
        }

        public bool InvalidParameter(MemberInfo Method, out string error)
        {
            if (TryGetParams(Method, out string parameters))
            {
                error = "Invalid argument, please provide one of the following: " + parameters;
            }
            else
            {
                error = "Invalid argument provided";
            }
            return false;
        }

        private async void LoadAndInitialiseLevel(int index)
        {
            await GameManager.LevelLoader.LoadLevel(index);
            GameManager.CurrentLevel = index;
            GameManager.InitialiseLevel();
        }
    }

    public struct Log
    {
        public string Condition;
        public string StackTrace;
        public LogType Type;

        public Log(string condition, string stackTrace, LogType type)
        {
            Condition = condition;
            StackTrace = stackTrace;
            Type = type;
        }
    }
    
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]

    class DescriptionAttribute : Attribute
    {
        public string DescriptionString { get; set; }
        public DescriptionAttribute(string descriptionString)
        {
            DescriptionString = descriptionString;
        }
    }
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]

    class ParameterAttribute : Attribute
    {
        public Type ParameterType { get; set; }
        public string ParameterString { get; set; }
        public int ParameterIndex;
        public int ParameterGroupIndex;
        public int ParameterAutofillMethod;

        public static Dictionary<Type, string> symbolKey = new Dictionary<Type, string>
        {
            {typeof(Nullable), ""},
            {typeof(bool), "!"},
            {typeof(string), "\"*\""},
            {typeof(int), "#"},
        };
        public ParameterAttribute(Type parameterType, string parameterString, int parameterIndex, int parameterAutofillMethod)
        {
            ParameterType = parameterType;
            ParameterString = parameterString;
            ParameterIndex = parameterIndex;
            ParameterGroupIndex = 0;
            ParameterAutofillMethod = parameterAutofillMethod;
        }
        public ParameterAttribute(Type parameterType, string parameterString, int parameterIndex, int parameterGroupIndex, int parameterAutofillMethod)
        {
            ParameterType = parameterType;
            ParameterString = parameterString;
            ParameterIndex = parameterIndex;
            ParameterGroupIndex = parameterGroupIndex;
            ParameterAutofillMethod = parameterAutofillMethod;
        }
    }
}
