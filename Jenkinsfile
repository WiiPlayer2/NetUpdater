pipeline {
    agent {
        label 'windows'
    }

    stages {
        stage ('Checkout') {
            steps {
                checkout scm
            }
        }

        stage ('Cleanup') {
            steps {
                powershell './build.ps1 -Target Clean'
            }
        }

        stage ('Build') {
            steps {
                powershell './build.ps1 -Target Pack -Configuration Release -SkipClean'
                powershell './build.ps1 -Target Pack -Configuration Debug -SkipClean'
            }
        }

        stage ('Test') {
            steps {
                powershell './build.ps1 -Target Test -Configuration Release'
            }
        }
        
        stage ('Archive') {
            steps {
                archiveArtifacts '/_build/**/*.nupkg'
                archiveArtifacts '/_build/**/*.zip'
            }
        }

        stage ('Publish') {
            environment {
                NUGET_API_KEY = credentials('nuget-feed-api-key')
                NUGET_SOURCE = "http://dark-link.info:5555/v3/index.json"
            }

            steps {
                powershell './build.ps1 -Target Publish -Configuration Release -SkipClean -SkipPack'
            }
        }
    }

    post {
        always {
            mstest testResultsFile: '_build/Release/**/*TestResults.xml'
        }
    }
}
