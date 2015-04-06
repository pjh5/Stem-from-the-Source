#pragma strict
@script ExecuteInEditMode()

function OnGUI()
{
	if(GUI.Button(Rect(Screen.width/2 - 80,Screen.height/2 + 125,160,40), 'PLAY'))
	{
		Application.LoadLevel('Level 1');
	}
}