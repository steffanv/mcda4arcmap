# MCDA4ArcMap

This add-in for the ArcMap geographic information system (GIS) offers multi-criteria decision analysis (MCDA) and visualization functions for vector data. The MCDA process is highly interactive and the results can be processed within ArcMap.

![Alt text](https://github.com/steffanv/mcda4arcmap/blob/master/doc/overview.jpg?raw=true "Overview")

PLEASE NOTE THE [DISCLAIMER BELOW](https://github.com/steffanv/mcda4arcmap/blob/master/README.md#disclaimer) AND THE STIPULATIONS UNDER THE [LICENSE TAB](LICENCE).

## Features
The add-in supports the following MCDA methods:

- Weighted Linear Combination (WLC)
- Ordered Weighted Averaging (OWA)
- Locally Weighted Linear Combination (LWLC)

The LWLC method includes the following neighborhood definitions:

- Queen
- Rook
- Distance
- K-Nearest Neighbors (KNN)
- Automatic (increases KNN until the result can be calculated)

Additional add-in features include:

- Maximum-score standardization procedure
- Score-range standardization procedure
- CSV export of the result table and parameters
- Classified and unclassed choropleth maps with diverging colour scheme
- Three modi to define the map rendering frequency

## Limitations

- Only polygon geometry is supported
- Criterion values must be numeric
- Rook contiguity for large data sets (> 1,000 polygons) is slow (several minutes processing time)
- Sessions are not persistent, MCDA results are lost when MCDA4ArcMap is closed - use right-click | Data | Export to save map layer containing results

## System Requirements

- Developed for ArcMap 10.1, ArcMap 10.2 or later and the .Net Framework 4.0 or later
- Download should appear as "ESRI AddIn File" - if ArcMap runs, this add-in will also work! Downloads can be found in the [releases section](https://github.com/steffanv/mcda4arcmap/releases)

## TOOLS

- [JetBrains](https://www.jetbrains.com/) provides a Resharper licence for the development of MCDA4ArcMap

## CREDITS

- The add-in (version 1.0) was developed by Steffan Voss, [Institute for Geoinformatics](https://www.uni-muenster.de/Geoinformatics/), University of Muenster, Germany, during his research visit in the [Department of Geography](http://www.ryerson.ca/geography/), Ryerson University, Toronto, Canada, from August 2012 to January 2013.
- The add-in is further developed by Steffan Voss.
- This project was initiated and supervised by [Dr. Claus Rinner](http://www.ryerson.ca/~crinner/), Dept. of Geography, Ryerson University.
- Partial funding for Steffan's research from Dr. Rinner's NSERC Discovery Grant is gratefully acknowledged.
- An earlier version of the LWLC tool for vector data in ArcMap was developed by Brad Carter for his [Master of Spatial Analysis (MSA)](http://www.ryerson.ca/graduate/programs/spatial/) degree at Ryerson University.

- The project was moved from [codeplex](https://mcda4arcmap.codeplex.com/).

## DISCLAIMER

The MCDA4ArcMap tool has garnered considerable interest from researchers and practitioners. While we are very pleased to see this, we need to remind all users that the tool is provided "as is" and that we cannot guarantee its fitness for a particular use. If you are using MCDA4ArcMap for a specific purpose, such as for a thesis or for real-world decision-making, it is your responsibility to verify that the MCDA algorithms are implemented in the way you expect and need them. As an open-source software project, MCDA4ArcMap allows you to do this directly in the source code. Please see the Apache License under the license tab above for the full legal details.
