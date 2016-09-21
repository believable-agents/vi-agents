using UnityEditor;
using ViAgents.Actions;
using UnityEngine;
using Rotorz.ReorderableList;

[CustomEditor(typeof(ActionSet))]
public class ActionSetEditor : Editor {
    public enum ActionType {
        BehaviorTree,
        FinalStateMachine
    }

    SerializedProperty actions;
    ActionType actionType;

    void OnEnable() {
        actions = serializedObject.FindProperty("actions");
    }
    
    public override void OnInspectorGUI() {
//        var actionSet = (ActionSet) target;
//
//        actionType = (ActionType) EditorGUILayout.EnumPopup("Type: ", actionType);
//        if(GUILayout.Button("Create"))
//        {
//            switch (actionType) {
//                case ActionType.BehaviorTree:
//                    actionSet.actions.Add(new ActionWithBT());
//                    break;
//            }
//        }


        serializedObject.Update();

        ReorderableListGUI.Title("Actions");
        ReorderableListGUI.ListField(actions);
        
        serializedObject.ApplyModifiedProperties();
    }
}

[CustomPropertyDrawer(typeof(Action))]
public class ActionEditor : PropertyDrawer
{
    public override float GetPropertyHeight(SerializedProperty prop, GUIContent label)
    {
        return 80;
    }
    
    public override void OnGUI(Rect position, SerializedProperty prop, GUIContent label)
    {
        position.height = 16;
        EditorGUI.PropertyField(position, prop.FindPropertyRelative("sensor")); 
        position.y += 20;
        EditorGUI.PropertyField(position, prop.FindPropertyRelative("sensorRequest")); 
        position.y += 20;
        EditorGUI.PropertyField(position, prop.FindPropertyRelative("BT")); 
        position.y += 20;
        //EditorGUI.PropertyField(position, prop.FindPropertyRelative("exitBT")); 
        //position.y += 20;
        //EditorGUI.PropertyField(position, prop.FindPropertyRelative("runForever")); 
        //position.y += 20;
        EditorGUI.PropertyField(position, prop.FindPropertyRelative("waitToFinish")); 

    }
}