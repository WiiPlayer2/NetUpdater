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
                powershell './build.ps1 -Target Pack -Configuration Release'
                powershell './build.ps1 -Target Pack -Configuration Debug'
            }
        }
        
        stage ('Archive') {
            steps {
                archiveArtifacts '/_build/**/*.zip'
            }
        }
    }
}
