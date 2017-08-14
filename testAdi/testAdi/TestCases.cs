using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace test
{
    public class TestCases
    {

        IWebDriver driver;
        WebDriverWait wait;
        string[] tasksToEnter;
        Tasks tasksObj;
        StreamWriter resultsFile;
        public bool isPassed { get; set; } //give option to read it from the main program ay any point
        public enum TestType { Toggle, Delete };
        public TestCases(IWebDriver _driver, WebDriverWait _wait, string[] _tasksToEnter, StreamWriter _resultsFile)
        {
            driver = _driver;
            wait = _wait;
            tasksToEnter = _tasksToEnter;
            resultsFile = _resultsFile;
            tasksObj = new Tasks();
            isPassed = true;
        }

      

        public void InsertStrings(bool check = true)
        {
            wait.Until(d => d.FindElement(By.Id("new-todo")));
            foreach (string nameToEnter in tasksToEnter)
            {
                AddItem(nameToEnter);
                if (check)
                    AssertTestCase(tasksObj.numOfActiveTasks, GetNumOfItems(), "num of tasks as written below updated");
            }
        }

        /*
         * this test insert items and verify that they indeed appear in the view of the “todo list” by name and count
	        and verify that the “X items left” as in “X” active tasks is correct 
         */
        public void Test_InsertItems()
        {

            InsertStrings();
            //after done entering all names, verify that indeed they are inside: 1. by number, 2 . by name

            IWebElement button = driver.FindElement(By.Id("todo-list"));
            ReadOnlyCollection<IWebElement> tasksList = button.FindElements(By.TagName("li"));
            if (tasksList.Count > 0)
            {
                if (AssertTestCase(tasksList.Count, tasksToEnter.Length, "number of items in list"))
                {

                    for (int i = 0; i < tasksList.Count; i++)
                    {
                        AssertTestCase(tasksToEnter[i], tasksList[i].Text, "name of task");
                    }
                }

            }

        }

       /*
        this test delets all items and verify correctness
        */
        public void Test_DeleteAllItems(State toggleState)
        {
            PrepareForToggleOrDelete(toggleState,TestType.Delete);
         
            ReadOnlyCollection<IWebElement> destroyList = wait.Until(d => d.FindElements(By.ClassName("destroy")));
          
            for (int i = 0; i < destroyList.Count; i++)
            {
                if (destroyList[i].Displayed)
                {
                    DeleteItem(destroyList[i], GetNameFromItemInTodoList(i+1), toggleState);

                    //check that num of items display was reduced\increased
                    AssertTestCase(tasksObj.numOfActiveTasks, GetNumOfItems(), "num of " + toggleState.ToString() + " tasks was updated after toggeling");
                }
            }
            //check both lists are updated
            Test_BothButtonsLists();
            GetAllViewState();

        }
        
        /* this test checks the "clear_completed" button and also the "Toggle-All" button 
         */
        public void Test_ClearCompleted()
        {
            
            IWebElement button = driver.FindElement(By.Id("clear-completed"));
            if (!button.Displayed)
            {
                //meaning that there are no items of completed
                //need to create "Complete" tasks
                ToggleAllActiveToCompleteButton();
                //Test lists after toggle all button
                Test_BothButtonsLists();
                button = driver.FindElement(By.Id("clear-completed"));
            }

            button.Click();
            tasksObj.ClearCompleted();
            
           
            //verify completed list is empty and Active list didn't change
            Test_BothButtonsLists();

        }

        public void Test_BothButtonsLists()
        {
            Test_ButtonsLists(State.Active);
            Test_ButtonsLists(State.Complete);
        }

        /* this test verifies the list shown is indeed as should be, count and content
         */
        public void Test_ButtonsLists(State state)
        {
            string listName = state == State.Complete ? "completed" : "active";

            IWebElement ListObj = driver.FindElement(By.CssSelector("a[href*='#!" + listName + "']"));
            if (ListObj.Displayed)
            {

                ListObj.Click();
                IWebElement todoList = driver.FindElement(By.Id("todo-list"));
                ReadOnlyCollection<IWebElement> tasksList = todoList.FindElements(By.TagName("li"));
                if (tasksList.Count > 0)
                {
                    //check names
                    string CheckListSucceedd = tasksObj.CompareLists(tasksList,state) ? "passed" : "failed";
                    resultsFile.WriteLine("link to " + listName + " list show the correct tasks" + ":  test " + CheckListSucceedd);

                }
                
            }
            else //verify that indeed there shouldn't be any tasks
            {
                if (state == State.Complete)
                    AssertTestCase(tasksObj.numOfCompletedTasks, 0, "indeed no tasks in list completed");
                else
                    AssertTestCase(tasksObj.numOfActiveTasks, 0, "indeed no tasks in list Active");
            }

        }

      
       

        public void Test_ToggleAllItems(State toggleState)
        {
            PrepareForToggleOrDelete(toggleState, TestType.Toggle);
            ReadOnlyCollection<IWebElement> toggleList = driver.FindElements(By.ClassName("toggle"));
           
            for (int i = 0; i < toggleList.Count; i++)
            {
                ToggleItem(toggleList[i], GetNameFromItemInTodoList(i+1), toggleState);
            
                //check that num of items display was reduced\increased
                AssertTestCase(tasksObj.numOfActiveTasks, GetNumOfItems(), "num of " + toggleState.ToString() + " tasks was updated after toggeling");

            }
            //check both lists are updated
            Test_BothButtonsLists();
            //return to all view state
            GetAllViewState();

        }


        /**************************  Help functions   **********************************************************/

        public string GetNameFromItemInTodoList(int position)
        {
            return driver.FindElement(By.XPath("//*[@id='todo-list']/li[" + position + "]/div/label")).Text;
        }


        public bool GetAllViewState()
        {
            IWebElement allview = driver.FindElement(By.CssSelector("a[href*='#!']"));
            if (allview.Displayed)
            {
                allview.Click();
                return true;
            }
            return false;
        }

        public void PrepareForToggleOrDelete(State toggleState, TestType type)
        {
            //move to "all" view in order to toggle all items

            if (!GetAllViewState())
            {
                InsertStrings(false);//after insertion,  they are all "Active"
                if ((toggleState == State.Active && type == TestType.Toggle) || (toggleState == State.Complete && type == TestType.Delete))
                //in order to toggle from complete to active state need to create all complete situation
                {
                    ToggleAllActiveToCompleteButton(); 
                }
                
            }

        }

        /* this function does ToggleAll Active items
       * and updates the simulator tasks list
       */
        public void ToggleAllActiveToCompleteButton()
        {
            driver.FindElement(By.Id("toggle-all")).Click();
            tasksObj.ToggleAllActiveToComplete();
        }

        public int GetNumOfItems()
        {
            IWebElement todoCount = driver.FindElement(By.Id("todo-count"));
            string[] splitString = todoCount.Text.Split();
            int numOfItems = Convert.ToInt32(splitString[0]);
            return numOfItems;
        }

        /* this function does AddItem
       * and updates the simulator tasks list
       */
        public void AddItem(string name)
        {
            driver.FindElement(By.Id("new-todo")).SendKeys(name + "\n");
            tasksObj.AddToList(name,State.Active);

        }
        /* this function does DeleteItem
      * and updates the simulator tasks list
      */
        public void DeleteItem(IWebElement element, string name, State state)
        {

            element.Click();
            tasksObj.RemoveFromList(name, state);
        }
     


        /* this function does ToggleItem
       * and updates the simulator tasks list
       */
        public void ToggleItem(IWebElement element, string name, State state)
        {
            element.Click();
            tasksObj.AddToList(name, state);
        }

        public bool AssertTestCase(Object expected, Object actual, string info)
        {
            if (!expected.Equals(actual))
            {
                string message = "testFailed, expected " + expected.ToString() + "  " + info + ", recieved " + actual.ToString();
                resultsFile.WriteLine(message);
                isPassed = false;
                return false;
            }
            resultsFile.WriteLine(info + ":  test passed");
            return true;
        }
        /***    End of Help functions   **********************************************************/
       
    }
}
