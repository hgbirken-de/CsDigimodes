using System;

namespace BlueDV
{
	// Token: 0x02000017 RID: 23
	internal class DMRRepair
	{
		// Token: 0x06000131 RID: 305 RVA: 0x0000C75C File Offset: 0x0000A95C
		public static void process_data(byte[] F_Slot_Data, byte[] F_Full_LC, byte[] F_Slot_Data_Status, byte[] F_DMR_SlotType)
		{
			bool[] array = new bool[196];
			bool[] array2 = new bool[196];
			for (uint num = 0U; num <= 11U; num += 1U)
			{
				array2[(int)(num * 8U)] = utils.byteToBitsBE(F_Slot_Data[(int)num])[7];
				array2[(int)(num * 8U + 1U)] = utils.byteToBitsBE(F_Slot_Data[(int)num])[6];
				array2[(int)(num * 8U + 2U)] = utils.byteToBitsBE(F_Slot_Data[(int)num])[5];
				array2[(int)(num * 8U + 3U)] = utils.byteToBitsBE(F_Slot_Data[(int)num])[4];
				array2[(int)(num * 8U + 4U)] = utils.byteToBitsBE(F_Slot_Data[(int)num])[3];
				array2[(int)(num * 8U + 5U)] = utils.byteToBitsBE(F_Slot_Data[(int)num])[2];
				array2[(int)(num * 8U + 6U)] = utils.byteToBitsBE(F_Slot_Data[(int)num])[1];
				array2[(int)(num * 8U + 7U)] = utils.byteToBitsBE(F_Slot_Data[(int)num])[0];
			}
			array2[96] = utils.byteToBitsBE(F_Slot_Data[12])[7];
			array2[97] = utils.byteToBitsBE(F_Slot_Data[12])[6];
			array2[98] = utils.byteToBitsBE(F_Slot_Data[20])[1];
			array2[99] = utils.byteToBitsBE(F_Slot_Data[20])[0];
			for (uint num2 = 21U; num2 <= 32U; num2 += 1U)
			{
				array2[(int)(num2 * 8U - 68U)] = utils.byteToBitsBE(F_Slot_Data[(int)num2])[7];
				array2[(int)(num2 * 8U + 1U - 68U)] = utils.byteToBitsBE(F_Slot_Data[(int)num2])[6];
				array2[(int)(num2 * 8U + 2U - 68U)] = utils.byteToBitsBE(F_Slot_Data[(int)num2])[5];
				array2[(int)(num2 * 8U + 3U - 68U)] = utils.byteToBitsBE(F_Slot_Data[(int)num2])[4];
				array2[(int)(num2 * 8U + 4U - 68U)] = utils.byteToBitsBE(F_Slot_Data[(int)num2])[3];
				array2[(int)(num2 * 8U + 5U - 68U)] = utils.byteToBitsBE(F_Slot_Data[(int)num2])[2];
				array2[(int)(num2 * 8U + 6U - 68U)] = utils.byteToBitsBE(F_Slot_Data[(int)num2])[1];
				array2[(int)(num2 * 8U + 7U - 68U)] = utils.byteToBitsBE(F_Slot_Data[(int)num2])[0];
			}
			for (uint num3 = 0U; num3 < 196U; num3 += 1U)
			{
				array[(int)num3] = array2[(int)(num3 * 181U % 196U)];
			}
			uint num4 = 0U;
			bool flag;
			do
			{
				flag = false;
				for (uint num5 = 0U; num5 <= 14U; num5 += 1U)
				{
					bool flag2 = array[(int)(1U + num5)] ^ array[(int)(16U + num5)] ^ array[(int)(46U + num5)] ^ array[(int)(76U + num5)] ^ array[(int)(91U + num5)];
					bool flag3 = array[(int)(1U + num5)] ^ array[(int)(16U + num5)] ^ array[(int)(31U + num5)] ^ array[(int)(61U + num5)] ^ array[(int)(91U + num5)] ^ array[(int)(106U + num5)];
					bool flag4 = array[(int)(1U + num5)] ^ array[(int)(16U + num5)] ^ array[(int)(31U + num5)] ^ array[(int)(46U + num5)] ^ array[(int)(76U + num5)] ^ array[(int)(106U + num5)] ^ array[(int)(121U + num5)];
					bool flag5 = array[(int)(1U + num5)] ^ array[(int)(31U + num5)] ^ array[(int)(61U + num5)] ^ array[(int)(76U + num5)] ^ array[(int)(121U + num5)];
					int num6 = ((false | (flag2 != array[(int)(136U + num5)])) ? 1 : 0) | ((flag3 != array[(int)(151U + num5)]) ? 2 : 0) | ((flag4 != array[(int)(166U + num5)]) ? 4 : 0) | ((flag5 != array[(int)(181U + num5)]) ? 8 : 0);
					if (num6 == 1)
					{
						array[(int)(136U + num5)] = !array[(int)(136U + num5)];
					}
					if (num6 == 2)
					{
						array[(int)(151U + num5)] = !array[(int)(151U + num5)];
					}
					if (num6 == 4)
					{
						array[(int)(166U + num5)] = !array[(int)(166U + num5)];
					}
					if (num6 == 5)
					{
						array[(int)(181U + num5)] = !array[(int)(181U + num5)];
					}
					if (num6 == 15)
					{
						array[(int)(1U + num5)] = !array[(int)(1U + num5)];
					}
					if (num6 == 7)
					{
						array[(int)(16U + num5)] = !array[(int)(16U + num5)];
					}
					if (num6 == 14)
					{
						array[(int)(31U + num5)] = !array[(int)(31U + num5)];
					}
					if (num6 == 5)
					{
						array[(int)(46U + num5)] = !array[(int)(46U + num5)];
					}
					if (num6 == 10)
					{
						array[(int)(61U + num5)] = !array[(int)(61U + num5)];
					}
					if (num6 == 13)
					{
						array[(int)(76U + num5)] = !array[(int)(76U + num5)];
					}
					if (num6 == 3)
					{
						array[(int)(91U + num5)] = !array[(int)(91U + num5)];
					}
					if (num6 == 6)
					{
						array[(int)(106U + num5)] = !array[(int)(106U + num5)];
					}
					if (num6 == 12)
					{
						array[(int)(121U + num5)] = !array[(int)(121U + num5)];
					}
					if (num6 != 0)
					{
						flag = true;
					}
				}
				for (uint num7 = 0U; num7 <= 8U; num7 += 1U)
				{
					bool flag6 = array[(int)(15U * num7 + 1U)] ^ array[(int)(15U * num7 + 2U)] ^ array[(int)(15U * num7 + 3U)] ^ array[(int)(15U * num7 + 4U)] ^ array[(int)(15U * num7 + 6U)] ^ array[(int)(15U * num7 + 8U)] ^ array[(int)(15U * num7 + 9U)];
					bool flag7 = array[(int)(15U * num7 + 2U)] ^ array[(int)(15U * num7 + 3U)] ^ array[(int)(15U * num7 + 4U)] ^ array[(int)(15U * num7 + 5U)] ^ array[(int)(15U * num7 + 7U)] ^ array[(int)(15U * num7 + 9U)] ^ array[(int)(15U * num7 + 10U)];
					bool flag8 = array[(int)(15U * num7 + 3U)] ^ array[(int)(15U * num7 + 4U)] ^ array[(int)(15U * num7 + 5U)] ^ array[(int)(15U * num7 + 6U)] ^ array[(int)(15U * num7 + 8U)] ^ array[(int)(15U * num7 + 10U)] ^ array[(int)(15U * num7 + 11U)];
					bool flag9 = array[(int)(15U * num7 + 1U)] ^ array[(int)(15U * num7 + 2U)] ^ array[(int)(15U * num7 + 3U)] ^ array[(int)(15U * num7 + 5U)] ^ array[(int)(15U * num7 + 7U)] ^ array[(int)(15U * num7 + 8U)] ^ array[(int)(15U * num7 + 11U)];
					int num8 = ((false | (flag6 != array[(int)(15U * num7 + 12U)])) ? 1 : 0) | ((flag7 != array[(int)(15U * num7 + 13U)]) ? 2 : 0) | ((flag8 != array[(int)(15U * num7 + 14U)]) ? 4 : 0) | ((flag9 != array[(int)(15U * num7 + 15U)]) ? 8 : 0);
					if (num8 == 1)
					{
						array[(int)(15U * num7 + 12U)] = !array[(int)(15U * num7 + 12U)];
					}
					if (num8 == 2)
					{
						array[(int)(15U * num7 + 13U)] = !array[(int)(15U * num7 + 13U)];
					}
					if (num8 == 4)
					{
						array[(int)(15U * num7 + 14U)] = !array[(int)(15U * num7 + 14U)];
					}
					if (num8 == 8)
					{
						array[(int)(15U * num7 + 15U)] = !array[(int)(15U * num7 + 15U)];
					}
					if (num8 == 9)
					{
						array[(int)(15U * num7 + 1U)] = !array[(int)(15U * num7 + 1U)];
					}
					if (num8 == 11)
					{
						array[(int)(15U * num7 + 2U)] = !array[(int)(15U * num7 + 2U)];
					}
					if (num8 == 15)
					{
						array[(int)(15U * num7 + 3U)] = !array[(int)(15U * num7 + 3U)];
					}
					if (num8 == 7)
					{
						array[(int)(15U * num7 + 4U)] = !array[(int)(15U * num7 + 4U)];
					}
					if (num8 == 14)
					{
						array[(int)(15U * num7 + 5U)] = !array[(int)(15U * num7 + 5U)];
					}
					if (num8 == 5)
					{
						array[(int)(15U * num7 + 6U)] = !array[(int)(15U * num7 + 6U)];
					}
					if (num8 == 10)
					{
						array[(int)(15U * num7 + 7U)] = !array[(int)(15U * num7 + 7U)];
					}
					if (num8 == 13)
					{
						array[(int)(15U * num7 + 8U)] = !array[(int)(15U * num7 + 8U)];
					}
					if (num8 == 3)
					{
						array[(int)(15U * num7 + 9U)] = !array[(int)(15U * num7 + 9U)];
					}
					if (num8 == 6)
					{
						array[(int)(15U * num7 + 10U)] = !array[(int)(15U * num7 + 10U)];
					}
					if (num8 == 12)
					{
						array[(int)(15U * num7 + 11U)] = !array[(int)(15U * num7 + 11U)];
					}
					if (num8 != 0)
					{
						flag = true;
					}
				}
				num4 += 1U;
			}
			while (flag && num4 < 5U);
			if (!flag || flag)
			{
				bool[] array3 = new bool[8];
				for (uint num9 = 4U; num9 <= 11U; num9 += 1U)
				{
					array3[(int)(num9 - 4U)] = array[(int)num9];
				}
				F_Full_LC[0] = utils.bitsToByteBE(array3);
				for (uint num10 = 16U; num10 <= 23U; num10 += 1U)
				{
					array3[(int)(num10 - 16U)] = array[(int)num10];
				}
				F_Full_LC[1] = utils.bitsToByteBE(array3);
				for (uint num11 = 24U; num11 <= 26U; num11 += 1U)
				{
					array3[(int)(num11 - 24U)] = array[(int)num11];
				}
				for (uint num12 = 31U; num12 <= 35U; num12 += 1U)
				{
					array3[(int)(num12 - 29U)] = array[(int)num12];
				}
				F_Full_LC[2] = utils.bitsToByteBE(array3);
				for (uint num13 = 36U; num13 <= 41U; num13 += 1U)
				{
					array3[(int)(num13 - 36U)] = array[(int)num13];
				}
				for (uint num14 = 46U; num14 <= 47U; num14 += 1U)
				{
					array3[(int)(num14 - 31U)] = array[(int)num14];
				}
				F_Full_LC[3] = utils.bitsToByteBE(array3);
				for (uint num15 = 48U; num15 <= 55U; num15 += 1U)
				{
					array3[(int)(num15 - 48U)] = array[(int)num15];
				}
				F_Full_LC[4] = utils.bitsToByteBE(array3);
				array3[0] = array[56];
				for (uint num16 = 61U; num16 <= 67U; num16 += 1U)
				{
					array3[(int)(num16 - 60U)] = array[(int)num16];
				}
				F_Full_LC[5] = utils.bitsToByteBE(array3);
				for (uint num17 = 68U; num17 <= 71U; num17 += 1U)
				{
					array3[(int)(num17 - 68U)] = array[(int)num17];
				}
				for (uint num18 = 76U; num18 <= 79U; num18 += 1U)
				{
					array3[(int)(num18 - 72U)] = array[(int)num18];
				}
				F_Full_LC[6] = utils.bitsToByteBE(array3);
				for (uint num19 = 80U; num19 <= 86U; num19 += 1U)
				{
					array3[(int)(num19 - 80U)] = array[(int)num19];
				}
				array3[7] = array[91];
				F_Full_LC[7] = utils.bitsToByteBE(array3);
				for (uint num20 = 92U; num20 <= 99U; num20 += 1U)
				{
					array3[(int)(num20 - 80U)] = array[(int)num20];
				}
				F_Full_LC[8] = utils.bitsToByteBE(array3);
				for (uint num21 = 100U; num21 <= 101U; num21 += 1U)
				{
					array3[(int)(num21 - 100U)] = array[(int)num21];
				}
				for (uint num22 = 106U; num22 <= 111U; num22 += 1U)
				{
					array3[(int)(num22 - 104U)] = array[(int)num22];
				}
				F_Full_LC[9] = utils.bitsToByteBE(array3);
				for (uint num23 = 112U; num23 <= 116U; num23 += 1U)
				{
					array3[(int)(num23 - 112U)] = array[(int)num23];
				}
				for (uint num24 = 121U; num24 <= 123U; num24 += 1U)
				{
					array3[(int)(num24 - 117U)] = array[(int)num24];
				}
				F_Full_LC[10] = utils.bitsToByteBE(array3);
				for (uint num25 = 124U; num25 <= 131U; num25 += 1U)
				{
					array3[(int)(num25 - 124U)] = array[(int)num25];
				}
				F_Full_LC[11] = utils.bitsToByteBE(array3);
			}
			for (uint num26 = 0U; num26 < 196U; num26 += 1U)
			{
				array2[(int)(num26 * 181U % 196U)] = array[(int)num26];
			}
		}
	}
}
