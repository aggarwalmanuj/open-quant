namespace Spider.Trading.OpenQuant3.Enums
{
    public enum StopPriceCalculationStrategy
    {
        FixedAmount,

        // Protective is to make sure price does not higher than yesterday's high or Today's Open
        ProtectiveStopBasedOnAtr,

        // Retrace from previous Lo Or Current Day's Open to make the entry
        RetracementEntryBasedOnAtr,

        OpeningGap,

        OpeningGapOrProtectiveStop,

        OpeningGapOrRetracementEntry,

        OpeningGapAndProtectiveStop,

        OpeningGapAndRetracementEntry,

        AbIfStopBasedOnAtr
    }
}