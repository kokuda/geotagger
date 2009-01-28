//////////////////////////////////////////////////////////////////////////////
//
//    This file is part of Geotagger: A tool for geotagging photographs
//    Copyright (C) 2009  Kaz Okuda (http://notions.okuda.ca)
//
//    Geotagger is free software: you can redistribute it and/or modify
//    it under the terms of the GNU General Public License as published by
//    the Free Software Foundation, either version 3 of the License, or
//    (at your option) any later version.
//
//    This program is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//    GNU General Public License for more details.
//
//    You should have received a copy of the GNU General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
//
//////////////////////////////////////////////////////////////////////////////

namespace ExifHeader
{
    public enum Tag
    {
        INTEROP_INDEX = 0x0001,
        INTEROP_VERSION = 0x0002,
        IMAGE_WIDTH = 0x0100,
        IMAGE_LENGTH = 0x0101,
        BITS_PER_SAMPLE = 0x0102,
        COMPRESSION = 0x0103,
        PHOTOMETRIC_INTERP = 0x0106,
        FILL_ORDER = 0x010A,
        DOCUMENT_NAME = 0x010D,
        IMAGE_DESCRIPTION = 0x010E,
        MAKE = 0x010F,
        MODEL = 0x0110,
        SRIP_OFFSET = 0x0111,
        ORIENTATION = 0x0112,
        SAMPLES_PER_PIXEL = 0x0115,
        ROWS_PER_STRIP = 0x0116,
        STRIP_BYTE_COUNTS = 0x0117,
        X_RESOLUTION = 0x011A,
        Y_RESOLUTION = 0x011B,
        PLANAR_CONFIGURATION = 0x011C,
        RESOLUTION_UNIT = 0x0128,
        TRANSFER_FUNCTION = 0x012D,
        SOFTWARE = 0x0131,
        DATETIME = 0x0132,
        ARTIST = 0x013B,
        WHITE_POINT = 0x013E,
        PRIMARY_CHROMATICITIES = 0x013F,
        TRANSFER_RANGE = 0x0156,
        JPEG_PROC = 0x0200,
        THUMBNAIL_OFFSET = 0x0201,
        THUMBNAIL_LENGTH = 0x0202,
        Y_CB_CR_COEFFICIENTS = 0x0211,
        Y_CB_CR_SUB_SAMPLING = 0x0212,
        Y_CB_CR_POSITIONING = 0x0213,
        REFERENCE_BLACK_WHITE = 0x0214,
        RELATED_IMAGE_WIDTH = 0x1001,
        RELATED_IMAGE_LENGTH = 0x1002,
        CFA_REPEAT_PATTERN_DIM = 0x828D,
        CFA_PATTERN1 = 0x828E,
        BATTERY_LEVEL = 0x828F,
        COPYRIGHT = 0x8298,
        EXPOSURETIME = 0x829A,
        FNUMBER = 0x829D,
        IPTC_NAA = 0x83BB,
        EXIF_OFFSET = 0x8769,
        INTER_COLOR_PROFILE = 0x8773,
        EXPOSURE_PROGRAM = 0x8822,
        SPECTRAL_SENSITIVITY = 0x8824,
        GPSINFO = 0x8825,
        ISO_EQUIVALENT = 0x8827,
        OECF = 0x8828,
        EXIF_VERSION = 0x9000,
        DATETIME_ORIGINAL = 0x9003,
        DATETIME_DIGITIZED = 0x9004,
        COMPONENTS_CONFIG = 0x9101,
        CPRS_BITS_PER_PIXEL = 0x9102,
        SHUTTERSPEED = 0x9201,
        APERTURE = 0x9202,
        BRIGHTNESS_VALUE = 0x9203,
        EXPOSURE_BIAS = 0x9204,
        MAXAPERTURE = 0x9205,
        SUBJECT_DISTANCE = 0x9206,
        METERING_MODE = 0x9207,
        LIGHT_SOURCE = 0x9208,
        FLASH = 0x9209,
        FOCALLENGTH = 0x920A,
        MAKER_NOTE = 0x927C,
        USERCOMMENT = 0x9286,
        SUBSEC_TIME = 0x9290,
        SUBSEC_TIME_ORIG = 0x9291,
        SUBSEC_TIME_DIG = 0x9292,
        WINXP_TITLE = 0x9c9b, // Windows XP - not part of exif standard.
        WINXP_COMMENT = 0x9c9c, // Windows XP - not part of exif standard.
        WINXP_AUTHOR = 0x9c9d, // Windows XP - not part of exif standard.
        WINXP_KEYWORDS = 0x9c9e, // Windows XP - not part of exif standard.
        WINXP_SUBJECT = 0x9c9f, // Windows XP - not part of exif standard.

        FLASH_PIX_VERSION = 0xA000,
        COLOR_SPACE = 0xA001,
        EXIF_IMAGEWIDTH = 0xA002,
        EXIF_IMAGELENGTH = 0xA003,
        RELATED_AUDIO_FILE = 0xA004,
        INTEROP_OFFSET = 0xA005,
        FLASH_ENERGY = 0xA20B,
        SPATIAL_FREQ_RESP = 0xA20C,
        FOCAL_PLANE_XRES = 0xA20E,
        FOCAL_PLANE_YRES = 0xA20F,
        FOCAL_PLANE_UNITS = 0xA210,
        SUBJECT_LOCATION = 0xA214,
        EXPOSURE_INDEX = 0xA215,
        SENSING_METHOD = 0xA217,
        FILE_SOURCE = 0xA300,
        SCENE_TYPE = 0xA301,
        CFA_PATTERN = 0xA302,
        CUSTOM_RENDERED = 0xA401,
        EXPOSURE_MODE = 0xA402,
        WHITEBALANCE = 0xA403,
        DIGITALZOOMRATIO = 0xA404,
        FOCALLENGTH_35MM = 0xA405,
        SCENE_CAPTURE_TYPE = 0xA406,
        GAIN_CONTROL = 0xA407,
        CONTRAST = 0xA408,
        SATURATION = 0xA409,
        SHARPNESS = 0xA40A,
        DISTANCE_RANGE = 0xA40C,

        // Special non-exif tags used for internal purposes
        // Use more than 16 bits to avoid collisions with real tags.
        EXIF_OFFSET_APPENDED = 0x10000,
    }
}
