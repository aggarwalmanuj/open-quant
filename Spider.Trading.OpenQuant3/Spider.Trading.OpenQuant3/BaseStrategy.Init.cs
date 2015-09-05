using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenQuant.API;
using Spider.Trading.OpenQuant3.Validators;

namespace Spider.Trading.OpenQuant3
{
    public abstract partial class BaseStrategy
    {
        private void SetAndValidateOrderTrigger()
        {
            OrderTriggerValidator.SetAndValidateValue(this);
        }

        protected void SetAndValidateValues()
        {
            // Validate time trigger
            TriggerTimeValidator.SetAndValidateValue(this);

            // Validate retry interval
            TriggerRetryTimeIntervalValidator.SetAndValidateValue(this);
        }


        private void StrategyInitialize()
        {

            try
            {
                HandleValidateInput();

                //HandleStrategyInitialization();

                IsInitialized = true;
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {

            }
        }


       
        private void StrategyReInitialize()
        {
            try
            {
                HandleValidateInput();

                //HandleStrategyReInitialization();

                IsReInitialized = true;
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {

            }
        } 
    }
}
