using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using Unity.Netcode;

public class DebugController : MonoBehaviour
{
    private bool showConsole = false;
    private bool showHelp = true;

    private bool focusTextField = false;
    private string input;

    private Vector2 scrollPos;

    private int TartgetFrameRate = 60;

    // Reference resolution for scaling (adjust this to your "perfect" resolution)
    private const float REFERENCE_HEIGHT = 360;

    // Scaled values (calculated based on screen height)
    private float consoleHeight => 30f * GetScaleFactor();
    private float helpBoxHeight => 100f * GetScaleFactor();
    private float helpBoxPadding => 5f * GetScaleFactor();
    private float itemHeight => 20f * GetScaleFactor();
    private float textFieldPadding => 10f * GetScaleFactor();
    private float textFieldHeight => 20f * GetScaleFactor();
    private float textFieldOffsetY => 5f * GetScaleFactor();

    //command definitions
    private static DebugCommand CommandInfo;

    public static DebugCommand StartHost;
    public static DebugCommand<string> StartClient;
    public static DebugCommand Disconnect;

    public static DebugCommand ShowFPS;
    public static DebugCommand<int> SetFPSTarget;

    public static DebugCommand ShowDebug;
    public static DebugCommand ShowNetworkDebug;

    public static DebugCommand ShowHelp;

    public List<object> commandList;

    [SerializeField] private GameObject FPSDisplay;

    /*
    IMPORTANT 
    when creating a command that needs a type not a void return (i.e. int, float, string, bool)
    must use a try and catch to handle invalid input errors 
    */

    //note need to make this into a unity package at some point -strip out project specific stuff first tho

    // Calculate scale factor based on screen height
    private float GetScaleFactor()
    {
        return Screen.height / REFERENCE_HEIGHT;
    }

    private void Awake()
    {
        //command setup
        //command format = new DebugCommand<type>("commandId", "command description", "command format (name<T>)", (type value) => { });
        CommandInfo = new DebugCommand("commandInfo", "Command description", "commandName <Value>", () =>
        {
            //does nothing - just an example
        });

        StartHost = new DebugCommand("startHost", "host a session", "startHost", () =>
        {
            Relay.instance.StartHost();
        });
        StartClient = new DebugCommand<string>("startClient", "connect to session", "startClient <lobby code>", (joinCode) =>
        {
            Relay.instance.JoinGame(joinCode);
        });
        Disconnect = new DebugCommand("disconnect", "disconnect form session", "disconnect", () =>
        {
            NetworkManager.Singleton.Shutdown();
        });

        ShowNetworkDebug = new DebugCommand("showNetworkDebug", "Toggle network debug info", "showNetworkDebug", () =>
        {
            ShowNetworkStats.instance.ToggleNetworkStats();
        });

        ShowHelp = new DebugCommand("showHelp", "show command list", "showHelp", () =>
        {
            showHelp = !showHelp;
        });

        ShowDebug = new DebugCommand("showDebug", "Toggle debug info", "showDebug", () =>
        {
            //Debug.unityLogger.logEnabled = !Debug.unityLogger.logEnabled;
            Debug.developerConsoleEnabled = !Debug.developerConsoleEnabled;
        });

        SetFPSTarget = new DebugCommand<int>("setFPSTarget", "set target frame rate", "setFPSTarget <int>", (fps) =>
        {
            Application.targetFrameRate = fps;
        });

        ShowFPS = new DebugCommand("showFPS", "Toggle showing frame rate", "showFPS", () =>
        {
            if (FPSDisplay == null) return;

            FPSDisplay.SetActive(!FPSDisplay.activeInHierarchy);
        });

        //command indexing
        commandList = new List<object>
        {
            CommandInfo,

            StartHost,
            StartClient,
            Disconnect,

            ShowFPS,
            SetFPSTarget,

            ShowNetworkDebug,
            ShowDebug,

            ShowHelp,
        };
    }

    private void Start()
    {
        Application.targetFrameRate = TartgetFrameRate;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha0))//toggle console with the 0 key
        {
            ToggleConsole();
        }
    }

    public void ToggleConsole()
    {
        showConsole = !showConsole;

        if (showConsole)
        {
            focusTextField = true;//set focus next frame
        }
        else
        {
            GUI.FocusControl(null);//remove focus when closing
        }
    }

    private void OnGUI()
    {
        if (!showConsole) return;

        //scale font size based on resolution
        GUI.skin.label.fontSize = Mathf.RoundToInt(12f * GetScaleFactor());
        GUI.skin.textField.fontSize = Mathf.RoundToInt(12f * GetScaleFactor());

        //check for 0 key to close console
        if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Alpha0)
        {
            ToggleConsole();
            return;
        }

        //check for enter key to process input
        if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Return)
        {
            ToggleConsole();
            ProcessInput(input);
            return;
        }

        float y = Screen.height - consoleHeight;

        if (showHelp)//draw help box
        {
            GUI.Box(new Rect(0, y - helpBoxHeight, Screen.width, helpBoxHeight), "");
            Rect viewport = new Rect(0, 0, Screen.width - 30 * GetScaleFactor(), itemHeight * commandList.Count);

            scrollPos = GUI.BeginScrollView(new Rect(0, y - helpBoxHeight, Screen.width, helpBoxHeight - 10 * GetScaleFactor()), scrollPos, viewport);

            for (int i = 0; i < commandList.Count; i++)
            {
                DebugCommandBase command = commandList[i] as DebugCommandBase;

                string lable = $"{command.commandFormat} - {command.commandDescription}";
                Rect lableRect = new Rect(helpBoxPadding, i * itemHeight, viewport.width - helpBoxPadding * 2, itemHeight);

                GUI.Label(lableRect, lable);
            }

            GUI.EndScrollView();
        }

        //draw console background and text field
        GUI.Box(new Rect(0, y, Screen.width, consoleHeight), "");
        GUI.SetNextControlName("ConsoleInput");
        input = GUI.TextField(new Rect(textFieldPadding, y + textFieldOffsetY, Screen.width - textFieldPadding * 2, textFieldHeight), input);

        if (focusTextField)//force focus on txt field when console opens
        {
            GUI.FocusControl("ConsoleInput");
            focusTextField = false;
        }
    }

    private void ProcessInput(string input)
    {
        if (input == null) return;
        string[] properties = input.Split(' ');//split input into properties

        for (int i = 0; i < commandList.Count; i++)
        {
            DebugCommandBase commandBase = commandList[i] as DebugCommandBase;//cast to base to access commandId

            //command execution depends on type provided
            if (input.Contains(commandBase.commandId))//check if input contains command id
            {
                if (commandList[i] as DebugCommand != null)//check if can cast to command
                {
                    (commandList[i] as DebugCommand).Execute();//cast and execute command
                }
                else if (commandList[i] as DebugCommand<int> != null)
                {
                    (commandList[i] as DebugCommand<int>).Execute(int.Parse(properties[1]));
                }
                else if (commandList[i] as DebugCommand<float> != null)
                {
                    (commandList[i] as DebugCommand<float>).Execute(float.Parse(properties[1]));
                }
                else if (commandList[i] as DebugCommand<string> != null)
                {
                    (commandList[i] as DebugCommand<string>).Execute(properties[1]);
                }
                else if (commandList[i] as DebugCommand<bool> != null)
                {
                    (commandList[i] as DebugCommand<bool>).Execute(bool.Parse(properties[1]));
                }
            }
        }
    }
}