package patches.buildTypes

import jetbrains.buildServer.configs.kotlin.v2019_2.*
import jetbrains.buildServer.configs.kotlin.v2019_2.ui.*

/*
This patch script was generated by TeamCity on settings change in UI.
To apply the patch, change the buildType with id = 'Build'
accordingly, and delete the patch script.
*/
changeBuildType(RelativeId("Build")) {
    params {
        add {
            param("env.Git_Branch", "${DslContext.settingsRoot.paramRefs.buildVcsBranch}")
        }
    }

    requirements {
        remove {
            equals("system.Octopus.AgentType", "Build-VS2019")
        }
        add {
            equals("system.Octopus.AgentType", "Build-VS2019", "RQ_1")
        }
        add {
            exists("DotNetCoreSDK3.1_Path")
        }
        add {
            equals("env.OS", "Windows_NT")
        }
        add {
            exists("DotNetFrameworkTargetingPack4.5.2_Path")
        }
    }

    expectDisabledSettings()
    updateDisabledSettings("RQ_1")
}
