<?xml version="1.0"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
  <xsl:template match="/">
    <Forecast>
    <Precipitation>
	<xsl:for-each select="//parameters/probability-of-precipitation">
		<xsl:choose>
			<xsl:when test="value[1] &gt; 0"><A><xsl:value-of select="value[1]"/></A></xsl:when>
			<xsl:otherwise><A>0</A></xsl:otherwise>
		</xsl:choose>
		<xsl:choose>
			<xsl:when test="value[2] &gt; 0"><A><xsl:value-of select="value[2]"/></A></xsl:when>
			<xsl:otherwise><A>0</A></xsl:otherwise>
		</xsl:choose>
    	</xsl:for-each>
    </Precipitation>
    <Low_Temp>
	<xsl:for-each select="//parameters/temperature">
		<xsl:choose>
			<xsl:when test="@type=&quot;minimum&quot;">
				<xsl:choose>
					<xsl:when test="value[1] &gt; 0"><A><xsl:value-of select="value[1]"/></A></xsl:when>
					<xsl:otherwise><A>0</A></xsl:otherwise>
				</xsl:choose>
				<xsl:choose>
					<xsl:when test="value[2] &gt; 0"><A><xsl:value-of select="value[2]"/></A></xsl:when>
					<xsl:otherwise><A>0</A></xsl:otherwise>
				</xsl:choose>
			</xsl:when>
		</xsl:choose>
    	</xsl:for-each>
    </Low_Temp>
    </Forecast>
  </xsl:template>
</xsl:stylesheet>
