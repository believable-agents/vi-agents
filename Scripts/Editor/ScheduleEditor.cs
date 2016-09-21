using UnityEngine;
using System.Collections;
using UnityEditor;
using ViAgents;
using ViAgents.Schedules;
using Rotorz.ReorderableList;


[CustomEditor(typeof(Schedule))]
public class ScheduleEditor : Editor {
    SerializedProperty items;

    void OnEnable() {
        items = serializedObject.FindProperty("items");
    }

    public override void OnInspectorGUI() {
        serializedObject.Update();
        
        ReorderableListGUI.Title("Scheduled Items");
        ReorderableListGUI.ListField(items);

        serializedObject.ApplyModifiedProperties();
    }
    
}

[CustomPropertyDrawer(typeof(ScheduledItem))]
public class ScheduleItemEditor : PropertyDrawer
{
    public override float GetPropertyHeight(SerializedProperty prop, GUIContent label)
    {
        return 40;
    }
    
    public override void OnGUI(Rect position, SerializedProperty prop, GUIContent label)
    {
        var width = position.width;
        var x = position.x;
        var labelWidth = 30;
        var timeWidth = 50;
        position.height = 16;


        var startHour = prop.FindPropertyRelative("startHour");
        var startMinute = prop.FindPropertyRelative("startMinutes");

        EditorGUI.LabelField(position, "Start:");
        position.width = timeWidth;
        position.x += labelWidth + 5;
        var time = EditorGUI.TextField(position, string.Format("{0:00}:{1:00}", startHour.intValue, startMinute.intValue));

        if (time.IndexOf(":") > 0)
        {
            var split = time.Split(':');
            int par;
            if (int.TryParse( split[0], out par)) {
                startHour.intValue = par;
            }
            if (int.TryParse( split[1], out par)) {
                startMinute.intValue = par;
            }
        }

        // place end
        var endHour = prop.FindPropertyRelative("endHour");
        var endMinute = prop.FindPropertyRelative("endMinutes");

        position.width = labelWidth;
        position.x += timeWidth + 10;

        EditorGUI.LabelField(position, "End:");
        position.width = timeWidth;
        position.x += labelWidth + 5;
        time = EditorGUI.TextField(position, string.Format("{0:00}:{1:00}", endHour.intValue, endMinute.intValue));
        
        if (time.IndexOf(":") > 0)
        {
            var split = time.Split(':');
            int par;
            if (int.TryParse( split[0], out par)) {
                endHour.intValue = par;
            }
            if (int.TryParse( split[1], out par)) {
                endMinute.intValue = par;
            }
        }

        //
        position.x = x;
        position.y += 20;
        position.width = width;

        var action = prop.FindPropertyRelative("action");
        EditorGUI.PropertyField(position, action); 


    }
}
