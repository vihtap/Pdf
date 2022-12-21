﻿namespace Melville.Icc.Model;

/// <summary>
/// Describes colorspaces that an ICC profile might convert to or from.  See Table 19 on page 21 of the ICC spec
/// </summary>
public enum ColorSpace : uint
{
    XYZ = 0X58595A20,
    Lab = 0x4c616220,
    Luv = 0x4c757620,
    Ycbr = 0x59436272,
    Yxy = 0x59787920,
    RGB = 0x52474220,
    GRAY = 0x47524159,
    HSV = 0x48535620,
    HLS = 0x484c5320,
    CMYK = 0X434D594B,
    CMY  = 0X434D5920,
    Col2 = 0x32434c52,
    Col3 = 0x33434c52,
    Col4 = 0x34434c52,
    Col5 = 0x35434c52,
    Col6 = 0x36434c52,
    Col7 = 0x37434c52,
    Col8 = 0x38434c52,
    Col9 = 0x39434c52,
    ColA = 0x41434c52,
    ColB = 0x42434c52,
    ColC = 0x43434c52,
    ColD = 0x44434c52,
    ColE = 0x45434c52,
    ColF = 0x46434c52,
};