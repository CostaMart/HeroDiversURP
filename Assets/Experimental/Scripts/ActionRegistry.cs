using System.Collections.Generic;

public static class ActionRegistry
{
    private static readonly Dictionary<string, ActionID> actionsByName = new();
    private static readonly Dictionary<int, ActionID> actionsById = new();
    private static int nextId = 1;

    // Azioni predefinite del sistema
    public static readonly ActionID START_PATROL = RegisterAction("StartPatrol");
    public static readonly ActionID CHASE = RegisterAction("Chase");
    public static readonly ActionID STOP = RegisterAction("Stop");
    public static readonly ActionID RESUME = RegisterAction("Resume");
    public static readonly ActionID ATTACK = RegisterAction("Attack");
    public static readonly ActionID WAIT_AND_START_PATROL = RegisterAction("WaitAndStartPatrol");
    public static readonly ActionID ROTATE_TO_TARGET = RegisterAction("RotateToTarget");
    public static readonly ActionID AIM_AT_TARGET = RegisterAction("AimAtTarget");
    public static readonly ActionID MOVE_TO = RegisterAction("MoveTo");
    public static readonly ActionID ADD_ENABLED_OBJECT = RegisterAction("AddEnabledObject");
    public static readonly ActionID REMOVE_DISABLED_OBJECT = RegisterAction("RemoveDisabledObject");
    public static readonly ActionID ENABLE_LOST_SCREEN = RegisterAction("EnableLostScreen");
    public static readonly ActionID ENABLE_WIN_SCREEN = RegisterAction("EnableWinScreen");
    public static readonly ActionID DROP_ITEM = RegisterAction("DropItem");
    public static readonly ActionID PLAY_SOUND = RegisterAction("PlaySound");
    public static readonly ActionID WALK = RegisterAction("Walk");
    public static readonly ActionID RUN = RegisterAction("Run");
    public static readonly ActionID STOP_ATTACK = RegisterAction("StopAttack");
    public static readonly ActionID GET_UP = RegisterAction("GetUp");
    public static readonly ActionID START_SPAWNING = RegisterAction("StartSpawning");
    public static readonly ActionID STOP_SPAWNING = RegisterAction("StopSpawning");
    public static readonly ActionID SPAWN_BATCH = RegisterAction("SpawnBatch");

    public static ActionID RegisterAction(string actionName)
    {
        if (actionsByName.TryGetValue(actionName, out ActionID existingAction))
        {
            return existingAction;
        }

        ActionID newAction = new(nextId++, actionName);
        actionsByName[actionName] = newAction;
        actionsById[newAction.id] = newAction;

        return newAction;
    }

    public static ActionID GetActionByName(string actionName)
    {
        return actionsByName.TryGetValue(actionName, out ActionID actionId) ? actionId : default;
    }

    public static ActionID GetActionById(int id)
    {
        return actionsById.TryGetValue(id, out ActionID actionId) ? actionId : default;
    }

    public static bool IsValidAction(ActionID actionId)
    {
        return actionsById.ContainsKey(actionId.id);
    }
}