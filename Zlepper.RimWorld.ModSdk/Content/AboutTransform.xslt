<?xml version = "1.0" encoding = "UTF-8"?>
<!-- xsl stylesheet declaration with xsl namespace: 
Namespace tells the xlst processor about which 
element is to be processed and which is used for output purpose only 
-->
<xsl:transform version="1.0"
               xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
    <xsl:output omit-xml-declaration="no" indent="yes" encoding="UTF-8"/>
    <xsl:strip-space elements="*"/>

    <xsl:param name="modName" select="'Bio Surgery'"/>
    <xsl:param name="packageId" select="'zlepper.biogurery'"/>
    <xsl:param name="includeHugsLib" select="'true'"/>

    <!-- Copy everything -->
    <xsl:template match="@*|node()">
        <xsl:copy>
            <xsl:copy-of select="@*"/>
            <xsl:apply-templates/>
        </xsl:copy>
    </xsl:template>

    <xsl:template match="/ModMetaData">
        <ModMetaData>
            <name>
                <xsl:value-of select="$modName"/>
            </name>
            <packageId>
                <xsl:value-of select="$packageId"/>
            </packageId>
            <xsl:copy-of select="@*|node()"/>
            <xsl:apply-templates select="." mode="dependencies"/>
            <xsl:apply-templates select="." mode="loadAfter"/>
        </ModMetaData>
    </xsl:template>

    <xsl:template match="/ModMetaData/packageId"/>
    <xsl:template match="/ModMetaData/name"/>

    <xsl:template match="/ModMetaData/modDependencies" mode="dependencies">
        <xsl:copy>
            <xsl:if test="$includeHugsLib = 'true'">
                <li>
                    <packageId>UnlimitedHugs.HugsLib</packageId>
                    <displayName>HugsLib</displayName>
                    <downloadUrl>https://github.com/UnlimitedHugs/RimworldHugsLib/releases/latest</downloadUrl>
                    <steamWorkshopUrl>steam://url/CommunityFilePage/818773962</steamWorkshopUrl>
                </li>
            </xsl:if>
        </xsl:copy>
    </xsl:template>


    <xsl:template match="/ModMetaData[not(modDependencies)]" mode="dependencies">
        <xsl:if test="$includeHugsLib = 'true'">
            <modDependencies>
                <li>
                    <packageId>UnlimitedHugs.HugsLib</packageId>
                    <displayName>HugsLib</displayName>
                    <downloadUrl>https://github.com/UnlimitedHugs/RimworldHugsLib/releases/latest</downloadUrl>
                    <steamWorkshopUrl>steam://url/CommunityFilePage/818773962</steamWorkshopUrl>
                </li>
            </modDependencies>
        </xsl:if>
    </xsl:template>


    <xsl:template match="/ModMetaData/loadAfter" mode="loadAfter">
        <loadAfter>
            <xsl:if test="$includeHugsLib = 'true'">
                <li>UnlimitedHugs.HugsLib</li>
            </xsl:if>
        </loadAfter>
    </xsl:template>

    <xsl:template match="/ModMetaData[not(loadAfter)]" mode="loadAfter">
        <xsl:if test="$includeHugsLib = 'true'">
            <loadAfter>
                <li>UnlimitedHugs.HugsLib</li>
            </loadAfter>
        </xsl:if>
    </xsl:template>

</xsl:transform>