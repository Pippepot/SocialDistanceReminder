using System;
using System.Windows;
using System.Media;


namespace CoronaCaution
{
    public enum CautionState {
        Informational,
        Fleeing,
        Panicing,
    }

    public class CautionStateMachine
    {
        readonly TextBit[] InformationalTexts = new TextBit[] {
            new TextBit("Be sure to practice social distancing", 0, 10),
            new TextBit("Stay Together, apart", 0, 10),
            new TextBit("Tip of the day: Drink bleach - Trump", 0, 10)
        };

        readonly TextBit[] FleeTexts = new TextBit[] {
            new TextBit("Hold your distance!", 0, 10),
            new TextBit("Don't come near me!", 0, 10),
            new TextBit("You haven't even sanitized. Don't touch me!", 0, 10)   
        };

        readonly TextBit[] PanicTexts = new TextBit[] {
            new TextBit("AAHAHAHHHAHAHHAAHAHAHHAHAHHAHAHAAAAHHAHHHHAAHAAAAAAAHHHHHAAAAAAHHHAHHAAAAAAAAAAAAAAAHHAHAHAHHA", 0.03, 2),
            new TextBit("STOP! STOP! STOP! STOP!", 0.03, 2)
        };

        CautionState state = CautionState.Informational;
        const double FleeDst = 500;
        const double PanicDst = 300;
        Random random;
        MainWindow window;

        public void Initialize(MainWindow mainWindow)
        {
            window = mainWindow;
            random = new Random(DateTime.Now.Second);
        }

        public void Update()
        {
            DetermineState();
            DetermineAction();
        }

        // Determines which state to be in based on the distance from the cursor to the window
        private void DetermineState()
        {
            CautionState oldState = state;
            double dst = window.GetDistance();

            if (dst < PanicDst)
            {
                state = CautionState.Panicing;
            }
            else if (dst < FleeDst)
            {
                state = CautionState.Fleeing;
            }
            else
            {
                state = CautionState.Informational;
            }

            if (oldState != state)
                StateChanged();

        }

        // Calls the appropriate method based on the current state
        private void DetermineAction()
        {
            switch (state)
            {
                case CautionState.Informational:
                    Inform();
                    break;
                case CautionState.Fleeing:
                    Flee();
                    break;
                case CautionState.Panicing:
                    Panic();
                    break;
                default:
                    break;
            }

            Run();
        }

        // Changes the texts and plays annoying sounds
        #region States

        int informationalTextIndex = 0;
        void Inform()
        {
            if (ChangeText(InformationalTexts[informationalTextIndex]))
                informationalTextIndex = GetNewTextIndex(InformationalTexts.Length, informationalTextIndex);
        }

        int fleeTextIndex = 0;
        void Flee()
        {
            PlaySound(SystemSounds.Hand, 3);
            if (ChangeText(FleeTexts[fleeTextIndex]))
                fleeTextIndex = GetNewTextIndex(FleeTexts.Length, fleeTextIndex);
        }

        int panicTextIndex = 0;
        void Panic()
        {
            
            PlaySound(SystemSounds.Asterisk, 2);
            if (ChangeText(PanicTexts[panicTextIndex]))
                panicTextIndex = GetNewTextIndex(PanicTexts.Length, panicTextIndex);
        }

        #endregion

        // Gets a new text that is not the same as the current
        int GetNewTextIndex(int indices, int currentIndex)
        {
            int newIndex = -1;
            while (newIndex == -1)
            {
                newIndex = random.Next(0, indices);

                if (newIndex == currentIndex)
                    newIndex = -1;
            }
            return newIndex;
        }


        string currentText = "";
        string currentTextToCome = "";
        double timeSinceLastCharacterDisplayed = 0;
        double textChangeTime = 0;

        // Changes the text over time and returns true when the text is displayed fully and the wait is over
        bool ChangeText(TextBit text)
        {
            // Reset when the text to display is not the same as the one currently displaying
            if (currentTextToCome != text.text)
            {
                currentTextToCome = text.text;
                currentText = "";
                timeSinceLastCharacterDisplayed = 0;
            }
                

            // Reset when the text is fully displayed
            if (currentText.Length >= currentTextToCome.Length)
            {
                // And the wait is over
                if (textChangeTime >= text.wait)
                {
                    currentText = "";
                    timeSinceLastCharacterDisplayed = 0;
                    textChangeTime = 0;
                    return true;
                }

                textChangeTime += window.deltaTime;
            }

            // Loop through every character
            for (int i = currentText.Length; i < currentTextToCome.Length; i++)
            {
                // Display as many characters as possible in one frame
                timeSinceLastCharacterDisplayed += window.deltaTime;

                if (timeSinceLastCharacterDisplayed > text.delay)
                {
                    timeSinceLastCharacterDisplayed -= text.delay;
                    currentText += currentTextToCome.Substring(i, 1);
                }
                else
                {
                    break;
                }

            }

            window.ChangeText(currentText);

            return false;
        }

        void StateChanged()
        {
            window.ChangeIcon(state);

            // Change the texts
            switch (state)
            {
                case CautionState.Informational:
                    informationalTextIndex = GetNewTextIndex(InformationalTexts.Length, informationalTextIndex);
                    break;
                case CautionState.Fleeing:
                    fleeTextIndex = GetNewTextIndex(FleeTexts.Length, fleeTextIndex);
                    break;
                case CautionState.Panicing:
                    panicTextIndex = GetNewTextIndex(PanicTexts.Length, panicTextIndex);
                    break;
                default:
                    break;
            }
        }

        // The actual running part
        double velocity = 0;
        const double dragForce = 0.90;

        void Run()
        {
            // These numbers somehow fits allright
            double velocityChange = 5000000 / Math.Pow(window.GetDistance(),2);
            velocityChange *= window.deltaTime;

            // When the cursor is so far away it is affecting the movement minimally, don't affect it at all
            if (velocityChange < .5)
                velocityChange = 0;

            velocity += velocityChange;
            velocity *= dragForce;

            // Cap the velocity so the box doesn't fly to the moon in one frame
            if (velocity > 50)
                velocity = 50;

            // Move in the opposite direction of the cursor
            Vector dir = window.GetDirection();
            dir.Normalize();

            // Make the box snake its way away
            dir.X += Math.Sin(DateTime.Now.Millisecond * 0.005);
            dir.Y += Math.Cos(DateTime.Now.Millisecond * 0.005);

            window.ChangePosition(window.GetPosition() + dir * velocity);
        }

        // Plays a systemsound, but not at the same time, because they will override eachother and sound horrible
        double timeLeftToPlaySound = 0;
        void PlaySound(SystemSound sound, double interval)
        {
            if (timeLeftToPlaySound <= 0)
            {
                sound.Play();
                timeLeftToPlaySound = interval;
            }

            timeLeftToPlaySound -= window.deltaTime;

        }
        

    }



}

