#pragma strict
@script ExecuteInEditMode()

function OnGUI()
{
	if(GUI.Button(Rect(675,450,160,40), 'PLAY'))
	{
		Application.LoadLevel('Level 1');
	}
}