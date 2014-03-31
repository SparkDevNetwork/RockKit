﻿// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Web.UI;

using Rock;
using Rock.Attribute;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Web.UI;
using Rock.Security;

namespace RockWeb.Blocks.Core
{
    [DisplayName( "Binary File Detail" )]
    [Category( "Core" )]
    [Description( "Shows the details of a particular binary file item." )]

    [BooleanField( "Show Binary File Type" )]
    [WorkflowTypeField( "Workflow", "An optional workflow to activate for any new file uploaded", false, "", "Advanced" )]
    public partial class BinaryFileDetail : RockBlock, IDetailBlock
    {
        #region Control Methods

        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            fsFile.FileUploaded += fsFile_FileUploaded;
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( !Page.IsPostBack )
            {
                string itemId = PageParameter( "BinaryFileId" );
                string typeId = PageParameter( "BinaryFileTypeId" );
                if ( !string.IsNullOrWhiteSpace( itemId ) )
                {
                    if ( string.IsNullOrWhiteSpace( typeId ) )
                    {
                        ShowDetail( "BinaryFileId", int.Parse( itemId ) );
                    }
                    else
                    {
                        ShowDetail( "BinaryFileId", int.Parse( itemId ), int.Parse( typeId ) );
                    }
                }
                else
                {
                    pnlDetails.Visible = false;
                }

                ddlBinaryFileType.Visible = GetAttributeValue( "ShowBinaryFileType" ).AsBoolean();
            }
            else
            {
                if ( pnlDetails.Visible )
                {
                    var binaryFile = new BinaryFile { BinaryFileTypeId = ddlBinaryFileType.SelectedValueAsInt() ?? 0 };
                    if ( binaryFile.BinaryFileTypeId > 0 )
                    {
                        binaryFile.LoadAttributes();
                        phAttributes.Controls.Clear();
                        Rock.Attribute.Helper.AddEditControls( binaryFile, phAttributes, false );
                    }
                }
            }
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Shows the edit.
        /// </summary>
        /// <param name="itemKey">The item key.</param>
        /// <param name="itemKeyValue">The item key value.</param>
        public void ShowDetail( string itemKey, int itemKeyValue )
        {
            ShowDetail( itemKey, itemKeyValue, null );
        }

        /// <summary>
        /// Shows the detail.
        /// </summary>
        /// <param name="itemKey">The item key.</param>
        /// <param name="itemKeyValue">The item key value.</param>
        /// <param name="binaryFileTypeId">The binary file type id.</param>
        public void ShowDetail( string itemKey, int itemKeyValue, int? binaryFileTypeId )
        {
            if ( !itemKey.Equals( "BinaryFileId" ) )
            {
                return;
            }

            var binaryFileService = new BinaryFileService();
            BinaryFile binaryFile = null;

            if ( !itemKeyValue.Equals( 0 ) )
            {
                binaryFile = binaryFileService.Get( itemKeyValue );
            }

            if ( binaryFile != null )
            {
                lActionTitle.Text = ActionTitle.Edit( binaryFile.BinaryFileType.Name ).FormatAsHtmlTitle();
            }
            else
            {
                binaryFile = new BinaryFile { Id = 0, IsSystem = false, BinaryFileTypeId = binaryFileTypeId };

                string friendlyName = BinaryFile.FriendlyTypeName;
                if ( binaryFileTypeId.HasValue )
                {
                    var binaryFileType = new BinaryFileTypeService().Get( binaryFileTypeId.Value );
                    if ( binaryFileType != null )
                    {
                        friendlyName = binaryFileType.Name;
                    }
                }

                lActionTitle.Text = ActionTitle.Add( friendlyName ).FormatAsHtmlTitle();
            }

            // initialize the fileUploader BinaryFileId to whatever file we are editing/viewing
            fsFile.BinaryFileId = binaryFile.Id;
            
            ShowBinaryFileDetail( binaryFile );
        }

        /// <summary>
        /// Shows the detail.
        /// </summary>
        /// <param name="binaryFile">The binary file.</param>
        public void ShowBinaryFileDetail( BinaryFile binaryFile )
        {
            pnlDetails.Visible = true;
            hfBinaryFileId.SetValue( binaryFile.Id );
            
            if ( binaryFile.BinaryFileTypeId.HasValue )
            {
                fsFile.BinaryFileTypeGuid = new BinaryFileTypeService().Get( binaryFile.BinaryFileTypeId ?? 0).Guid;
            }

            tbName.Text = binaryFile.FileName;
            tbDescription.Text = binaryFile.Description;
            tbMimeType.Text = binaryFile.MimeType;
            ddlBinaryFileType.SetValue( binaryFile.BinaryFileTypeId );

            // render UI based on Authorized and IsSystem
            bool readOnly = false;

            nbEditModeMessage.Text = string.Empty;
            if ( !IsUserAuthorized( Authorization.EDIT ) )
            {
                readOnly = true;
                nbEditModeMessage.Text = EditModeMessage.ReadOnlyEditActionNotAllowed( BinaryFile.FriendlyTypeName );
            }

            phAttributes.Controls.Clear();
            binaryFile.LoadAttributes();

            if ( readOnly )
            {
                lActionTitle.Text = ActionTitle.View( BinaryFile.FriendlyTypeName );
                btnCancel.Text = "Close";
                Rock.Attribute.Helper.AddDisplayControls( binaryFile, phAttributes );
            }
            else
            {
                Rock.Attribute.Helper.AddEditControls( binaryFile, phAttributes, true );
            }

            tbName.ReadOnly = readOnly;
            tbDescription.ReadOnly = readOnly;
            tbMimeType.ReadOnly = readOnly;
            ddlBinaryFileType.Enabled = !readOnly;

            btnSave.Visible = !readOnly;
        }

        #endregion

        #region Edit Events

        /// <summary>
        /// Handles the Click event of the btnCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnCancel_Click( object sender, EventArgs e )
        {
            NavigateToParentPage();
        }

        /// <summary>
        /// Handles the Click event of the btnSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnSave_Click( object sender, EventArgs e )
        {
            BinaryFile binaryFile;
            BinaryFileService binaryFileService = new BinaryFileService();
            AttributeService attributeService = new AttributeService();

            int binaryFileId = int.Parse( hfBinaryFileId.Value );

            if ( binaryFileId == 0 )
            {
                binaryFile = new BinaryFile();
                binaryFileService.Add( binaryFile, CurrentPersonAlias );
            }
            else
            {
                binaryFile = binaryFileService.Get( binaryFileId );
            }

            // if a new file was uploaded, copy the uploaded file to this binaryFile (uploaded files are always new temporary binaryFiles)
            if ( fsFile.BinaryFileId != binaryFile.Id)
            {
                var uploadedBinaryFile = binaryFileService.Get(fsFile.BinaryFileId ?? 0);
                if (uploadedBinaryFile != null)
                {
                    binaryFile.BinaryFileTypeId = uploadedBinaryFile.BinaryFileTypeId;
                    if ( uploadedBinaryFile.Data != null )
                    {
                        binaryFile.Data = binaryFile.Data ?? new BinaryFileData();
                        binaryFile.Data.Content = uploadedBinaryFile.Data.Content;
                    }
                }
            }

            binaryFile.IsTemporary = false;
            binaryFile.FileName = tbName.Text;
            binaryFile.Description = tbDescription.Text;
            binaryFile.MimeType = tbMimeType.Text;
            binaryFile.BinaryFileTypeId = ddlBinaryFileType.SelectedValueAsInt();

            binaryFile.LoadAttributes();
            Rock.Attribute.Helper.GetEditValues( phAttributes, binaryFile );

            if ( !Page.IsValid )
            {
                return;
            }

            if ( !binaryFile.IsValid )
            {
                // Controls will render the error messages                    
                return;
            }

            RockTransactionScope.WrapTransaction( () =>
            {
                binaryFileService.Save( binaryFile, CurrentPersonAlias );
                binaryFile.SaveAttributeValues( CurrentPersonAlias );
            } );

            NavigateToParentPage();
        }

        #endregion

        #region Activities and Actions

        /// <summary>
        /// Handles the FileUploaded event of the fsFile control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void fsFile_FileUploaded( object sender, EventArgs e )
        {
            using ( new Rock.Data.UnitOfWorkScope() )
            {
                var binaryFileService = new BinaryFileService();
                BinaryFile binaryFile = null;
                if ( fsFile.BinaryFileId.HasValue )
                {
                    binaryFile = binaryFileService.Get( fsFile.BinaryFileId.Value );
                }

                if ( binaryFile != null )
                {
                    if ( !string.IsNullOrWhiteSpace( tbName.Text ) )
                    {
                        binaryFile.FileName = tbName.Text;
                    }

                    // set binaryFile.Id to original id since the UploadedFile is a temporary binaryFile with a different id
                    binaryFile.Id = hfBinaryFileId.ValueAsInt();
                    binaryFile.Description = tbDescription.Text;
                    binaryFile.BinaryFileTypeId = ddlBinaryFileType.SelectedValueAsInt();
                    if ( binaryFile.BinaryFileTypeId.HasValue )
                    {
                        binaryFile.BinaryFileType = new BinaryFileTypeService().Get( binaryFile.BinaryFileTypeId.Value );
                    }

                    binaryFile.LoadAttributes();
                    Rock.Attribute.Helper.GetEditValues( phAttributes, binaryFile );

                    // Process uploaded file using an optional workflow
                    Guid workflowTypeGuid = Guid.NewGuid();
                    if ( Guid.TryParse( GetAttributeValue( "Workflow" ), out workflowTypeGuid ) )
                    {
                        var workflowTypeService = new WorkflowTypeService();
                        var workflowType = workflowTypeService.Get( workflowTypeGuid );
                        if ( workflowType != null )
                        {
                            var workflow = Workflow.Activate( workflowType, binaryFile.FileName );

                            List<string> workflowErrors;
                            if ( workflow.Process( binaryFile, out workflowErrors ) )
                            {
                                binaryFile = binaryFileService.Get( binaryFile.Id );

                                if ( workflowType.IsPersisted )
                                {
                                    var workflowService = new Rock.Model.WorkflowService();
                                    workflowService.Add( workflow, CurrentPersonAlias );
                                    workflowService.Save( workflow, CurrentPersonAlias );
                                }
                            }
                        }
                    }
                    
                    ShowBinaryFileDetail( binaryFile );
                }
            }
        }

        #endregion
    }
}