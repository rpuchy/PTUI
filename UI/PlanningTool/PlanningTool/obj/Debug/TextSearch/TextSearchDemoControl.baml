   M S B A M L   `   `   `  ����  PlanningTool@    :TreeViewWithViewModelDemo.TextSearch.TextSearchDemoControl   5      W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5      R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5        +� FocusedElement  5      ��  �� ElementName$ searchTextBox��.��5      ]� .+�5	      �� $��Bottom=�5
      
�������$��1,0,1,1q�5      ��5      �� 5   
    �� 
StartPoint$	 0,0&�6    �� EndPoint$	 0,1&�6/   .��5      �  � Color$ 	#33000000��6    � Offset$ 0Q�6+   � 5      $ 	#11000000��6   $ 1Q�6+   5      ._�5      �� $��Center=�5   	   $��4q�5   	    �� Orientation$ 
Horizontal=�5   	   .+�5   
   �� $��Search for:��6   �� 5   
   -   5         ��Name$ searchTextBox��5      $	��6,0q�5      $	��150��5       ��5      �� *
SearchText+ �� UpdateSourceTrigger$ PropertyChanged=�5       �� 5"   
   $��_Find��5$      $	��8,0q�5%      ��5#      �� *SearchCommand+5%      5'   	   5(      R� 5+      -   5        	 R�Name$	 EngineObjectViewTree��5+   7   ��6   �� *FirstGeneration+��5,      �� 50   
   
 �� 
TargetType"
 Q�6   .�51      ��  �� Property �@Q�
IsExpanded �� Value 6)   �� *
IsExpanded+ �� Mode$ TwoWay=��� 52       �@Q�
IsSelected 6)   �� *
IsSelected+$ TwoWay=��� 53       � ��$ Normal"�6)    �� Triggers 54      P� 55       P� Property �@Q�
IsSelected P� Value . . �56      ��  � ��$
 Bold"�6-   57      59      ��5<      �� 5=   
    �� ItemsSource 6#   �� *
Children+.R�5>      �� ��6   �� *Name+5?      5A      5C      5D      