GradeOCR
========

This is a collection of grade register recognition that I wrote in 2014. It includes facility to generate/recognize artifact-resistant QR-code-like codes (AR-codes) in project ARCode, facility to recognize tables in provided image in project TableOCR, and facility to recognize handwritten grades (2, 3, 4 or 5) in sub-project GradeOCR. All the code should be runnable as-is (no external dependencies) on at least .Net 4 and VS 2010.

example_register.jpg file in the project root demonstrates the example register that can be processed and recognized by this software.

ARCode
------
QR-codes are optimized for recognition in bad conditions (bad lighting, blurring, etc) but they require superb printing quality, even the slightest glitches result in recognition failure. AR-code is expected to be used with scanners (that is, it doesn't handle perspective projection) and aims to trade recognition speed for recognition stability. For this, it uses "finder circles" (as opposed to square "finder patterns" in QR-code), and excessively redundant Reed-Solomon error correction in data matrix (32 bits to store the data, 224 bits to store error correction data). As was shown by production use, AR-code was resistant to just about any artifacts possible - bad printing quality (stripes, dots, heavy random printer noise), paper folds, pencil marks right on the code, etc.

AR-code is slow, but not *that* slow - recognition process took <1s on 8MPIX image (with code size ~200px).

Program.cs has runnable main entry point, which prompts user for selecting the input image, and proceeds to showing debug window with recognition steps and the result.

TableOCR
--------
This component was developed to recognize table structure in input image, given bad printing quality conditions. It uses variation of Hough transform and several healing heuristics to guess table rows. Again, was shown to tolerate *really* bad usage conditions.

Program.cs has runnable main entry point, which prompts user for selecting the input image, and proceeds to showing debug window with recognition steps and the result. On the final image, double-clicking on some grade cell reveals the grade recognition dalog.

GradeOCR
--------
This component handles recognition of hand-written grades (one of four possibilities - 2, 3, 4, 5). It uses neural network, trained on the database of over 40000 manually recognized grades.