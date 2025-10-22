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

    private int consoleHeight = 30;
    private Vector2 scrollPos;

    //command definitions
    private static DebugCommand CommandInfo;

    public static DebugCommand StartHost;
    public static DebugCommand<string> StartClient;
    public static DebugCommand Disconnect;

    public static DebugCommand ShowNetworkDebug;

    public static DebugCommand ShowHelp;

    public List<object> commandList;

    /*
    IMPORTANT 
    when creating a command that needs a type not a void return (i.e. int, float, string, bool)
    must use a try and catch to handle invalid input errors 
    */

    //note need to make this into a unity package at some point -strip out project specific stuff first tho

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

        ShowNetworkDebug = new DebugCommand("showNetworkDebug", "show network debug info", "showNetworkDebug", () =>
        {
            ShowNetworkStats.instance.ToggleNetworkStats();
        });

        ShowHelp = new DebugCommand("showHelp", "show command list", "showHelp", () =>
        {
            showHelp = !showHelp;
        });

        //command indexing
        commandList = new List<object>
        { 
            CommandInfo,
            StartHost,
            StartClient,
            Disconnect,
            ShowNetworkDebug,
            ShowHelp
        };
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.BackQuote))//toggle console with the ` key
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
        //check for ` key to close console
        if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.BackQuote)
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
            GUI.Box(new Rect(0, y - 100, Screen.width, 100), "");
            Rect viewport = new Rect(0, 0, Screen.width - 30, 20 * commandList.Count);

            scrollPos = GUI.BeginScrollView(new Rect(0, y - 100, Screen.width, 90), scrollPos, viewport);

            for (int i = 0; i < commandList.Count; i++)
            {
                DebugCommandBase command = commandList[i] as DebugCommandBase;
                
                string lable = $"{command.commandFormat} - {command.commandDescription}";
                Rect lableRect = new Rect(5, i * 20, viewport.width - 10, 20);

                GUI.Label(lableRect, lable);
            }

            GUI.EndScrollView();
        }

        //draw console background and text field
        GUI.Box(new Rect(0, y, Screen.width, 30), "");
        GUI.SetNextControlName("ConsoleInput");
        input = GUI.TextField(new Rect(10, y + 5, Screen.width - 20, 20), input);

        if (focusTextField)//force focus on txt field when console opens
        {
            GUI.FocusControl("ConsoleInput");
            focusTextField = false;
        }
    }

    private void ProcessInput(string input)
    {
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
