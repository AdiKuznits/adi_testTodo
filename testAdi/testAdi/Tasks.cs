using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenQA.Selenium;

namespace test
{
    class Tasks
    {
        List<string> Active;
        List<string> Completed;
        public int numOfActiveTasks { get; set; }
        public int numOfCompletedTasks { get; set; }

        //Ctor
        public Tasks()
        {
            Active = new List<string>();
            numOfActiveTasks = 0;
            Completed = new List<string>();
            numOfCompletedTasks = 0;
        }

     
        public void ChangeListAll(List<string> AddToList, List<string> RemoveFromList)
        {
            //move add items from "remove From" into "Addto"
            foreach (string task in RemoveFromList)
            {
                AddToList.Add(task);
            }
            RemoveFromList.Clear();
            UpdateCounters();
        }

        private void UpdateCounters()
        {
            //update counters
            numOfActiveTasks = Active.Count;
            numOfCompletedTasks = Completed.Count;
        }

        //adding to Active list meaning removing from Completed if exists
        public void AddToList(string name,State state)
        {
            List<string> AddTo = state == State.Complete ? Completed : Active;
            List<string> RemoveFrom = state == State.Complete ? Active : Completed;

            AddTo.Add(name);
            if (RemoveFrom.Count > 0)
            {
                RemoveFrom.Remove(name);

            }
            UpdateCounters();
        }

      
        public void RemoveFromList(String name, State state)
        {
            List<string> toRemove = state == State.Complete ? Completed : Active;
            int numOfTasksToCompare = state == State.Complete ? numOfCompletedTasks : numOfActiveTasks;

            toRemove.Remove(name);
            UpdateCounters();
           
            
        }

        public void ToggleAllActiveToComplete()
        {
            ChangeListAll(Completed, Active);
               
        }
        public void ClearCompleted()
		{
			if (Completed.Count > 0)
			{
				Completed.Clear();
				numOfCompletedTasks = 0;
			}
		}

        public bool CompareLists(ReadOnlyCollection<IWebElement> todoList, State state)
        {
            List<string> toCompare = state == State.Complete ? Completed : Active;
            int numOfTasksToCompare = state == State.Complete ? numOfCompletedTasks : numOfActiveTasks;

            if (todoList.Count != numOfTasksToCompare)
                return false;
            int i = 0;
            foreach (IWebElement name in todoList)
            {
                if (name.Text != toCompare[i])
                    return false;
                i++;
            }
            return true;
        }

		


		

    }
}
