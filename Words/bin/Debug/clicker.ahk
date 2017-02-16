;Loop, Read, clickerInput.txt
;{
;	
;	MsgBox Field number %A_Index% is %A_LoopReadLine%
;}

;MouseClickDrag, Left, 5, 5, 10, 10, 70

CoordMode, mouse, Screen
;Click 800, 400

first := 0
second := 0

Loop, read, clickerInput2.txt
{
	Loop, parse, A_LoopReadLine, %A_Tab%
    {
        ;MsgBox, Field number %A_Index% is %A_LoopField%.
		coordsString = %A_LoopReadLine%
		StringSplit, wordCoords, coordsString, %A_Space%
		
		Loop, %wordCoords0%
		{
			this_coord := wordCoords%a_index%
			;MsgBox, Color number %a_index% is %this_coord%.
			
			divisible := Mod(a_index, 2)
			
			if (divisible = 0)
			{
				;Click wordCoords$a_index%, wordCoords$a_index + 1%
				second = wordCoords%a_index%
			}
			else
			{
				first = wordCoords%a_index%
			}
			
			MsgBox, %first%
			MsgBox, %second%
		}
    }
}

;SendEvent {Click 6, 52, down}{click 45, 52, up}